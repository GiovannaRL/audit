using AForge.Video;
using AForge.Video.DirectShow;
using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using xPlannerCommon.Models;

namespace OfflineXPlanner
{
    public partial class AddPicture : Form
    {
        FilterInfoCollection _filterInfoCollection;
        FilterInfo _currentlVideoCaptureDevice;
        VideoCaptureDevice _videoCaptureDevice;
        private int _iDesignWidth = 755;
        private int _iControlWidth = 275;
        List<InventoryImage> _currentImages = new List<InventoryImage>();    
        List<string> _tempImagesPath = new List<string>();

        Main _mainForm;
        int _projectId;
        int _departmentId;
        int _roomId;
        int? _inventoryId;
        Inventory _inventoryItem;

        private class InventoryImage
        {
            public Bitmap Image { get; set; }
            public PhotoType Type { get; set; }

            public InventoryImage(Bitmap image, PhotoType type )
            {
                Image = image;
                Type = type;                
            }

        } 

        public AddPicture(Main form, int projectId, int departmentId, int roomId, int? inventoryId = null)
        {
            InitializeComponent();
            this.CenterToParent();
            this._mainForm = form;
            this._projectId = projectId;
            this._departmentId = departmentId;
            this._roomId = roomId;
            this._inventoryId = inventoryId;

            btnCaptureTagLeft.Visible = this._inventoryId != null;
            btnCaptureTagRight.Visible = this._inventoryId != null;

            if (_inventoryId != null)
            {
                InventoryDAO inventories = new InventoryDAO();
                _inventoryItem = inventories.GetRoomInventories(projectId, departmentId, roomId).Where(x => x.Id == _inventoryId).FirstOrDefault();
            }

        }

        private void cbCameraSelect_SelectedIndexChanged(object sender, EventArgs e)
        {            
             StopVideoCaptureDevice();
            _videoCaptureDevice = new VideoCaptureDevice(_filterInfoCollection[cboCameraSelect.SelectedIndex].MonikerString);
            _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            _currentlVideoCaptureDevice = _filterInfoCollection[cboCameraSelect.SelectedIndex];
            _videoCaptureDevice.Start();

        }

        private void VideoCaptureDevice_NewFrame(Object sender, NewFrameEventArgs e)
        {
            pictureBox1.Image = (Bitmap)e.Frame.Clone();
        }

        private void StopVideoCaptureDevice()
        {
            if (_videoCaptureDevice != null && _videoCaptureDevice.IsRunning)
            {
                _videoCaptureDevice.NewFrame -= VideoCaptureDevice_NewFrame;
                _videoCaptureDevice.SignalToStop();
                _videoCaptureDevice = null;
            }
        }

        private void btnCaptureImage_Click(object sender, EventArgs e)
        {
            var photoType = PhotoType.Photo;
            if (_inventoryId != null)
            {
                if (_inventoryItem != null & !_currentImages.Exists(x => x.Type == PhotoType.Asset) & string.IsNullOrEmpty(_inventoryItem.PhotoFile))
                    photoType = PhotoType.Asset;
            }

            CaptureImage(photoType);
        }

        private void btnCaptureTagImage_Click(object sender, EventArgs e)
        {
            foreach (var image in _currentImages)
            {
                if (image.Type == PhotoType.Tag)
                    image.Type = PhotoType.Photo;
            }
            CaptureImage(PhotoType.Tag);
        }

        private void CaptureImage(PhotoType type)
        {
            Clipboard.SetImage(pictureBox1.Image);
            Bitmap tempBmp = (Bitmap)Clipboard.GetImage();
            InventoryImage currentImg = new InventoryImage(tempBmp, type);
            _currentImages.Add(currentImg); 
            AddImageToPanel();
            Clipboard.Clear();

            ICameraDAO cameraDAO = new CameraDAO();
            cameraDAO.UpdateLastSelectedCamera(_currentlVideoCaptureDevice.Name);
            RefreshControlStatus();
        }

        public void AddImageToPanel()
        {
            PictureBox pb = new PictureBox();
            pb.Size = new Size(525, 300);
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.Image = (Bitmap)Clipboard.GetImage();
            pb.Click += new EventHandler(OpenImage_click);
            panelImages.Controls.Add(pb);
            panelImages.Controls.SetChildIndex(pb, 0);
        }

        private void OpenImage_click(object sender, EventArgs e)
        {
            
            if (sender is PictureBox pb)
            {
                Image tempImage = pb.Image;
                string tempPath = Path.ChangeExtension(
                    Path.Combine(Path.GetTempPath(), 
                    Path.GetTempFileName()), ".Jpeg"
                );
                _tempImagesPath.Add(tempPath);
                tempImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }           

        }

        private void DeleteTemporaryImages()
        {
            foreach (string path in _tempImagesPath)
            {
                File.Delete(path);
            }
        }

        private void btnRemoveImagens_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            panelImages.Controls.Clear();
            _currentImages.Clear();
            DeleteTemporaryImages();
            RefreshControlStatus();
        }

        private void SetPicture(Image img)
        {
            Bitmap temp = ((Bitmap)(img)).Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.BeginInvoke(new MethodInvoker(
                delegate ()
                {
                    pictureBox1.Image = temp;
                }));
            }
            else
            {
                pictureBox1.Image = temp;
            }

        }
        private void AddPicture_Load(object sender, EventArgs e)
        {
            _iDesignWidth = this.Width;
            ICameraDAO cameraDAO = new CameraDAO();
            string lastSelectedCameraName = cameraDAO.GetLastSelectedCamera();
            int selectedCameraIndex = 0;

            _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);         
            if (_filterInfoCollection.Count <= 0)
            {
                MessageBox.Show("Could not find any cameras available on your system to take pictures");
                CloseWindow();
            }
            for (int i = 0; i < _filterInfoCollection.Count; i++)
            {
                cboCameraSelect.Items.Add(_filterInfoCollection[i].Name);
                if (lastSelectedCameraName == _filterInfoCollection[i].Name)
                    selectedCameraIndex = i;
            }

            cboCameraSelect.SelectedIndex = selectedCameraIndex;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_currentImages.Count > 0)
            {
                if (MessageBox.Show("If you close this window one or more images are going to be lost. Are you sure you want to close it?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;  
                }
            }


            StopVideoCaptureDevice();
            CloseWindow();
        }

        private void CloseWindow()
        {
            DeleteTemporaryImages();
            StopVideoCaptureDevice();
            this.Close();            
        }

        private void btnAddImages_Click(object sender, EventArgs e)
        {
            if (_currentImages.Count > 0)
            {
                var path = _mainForm.RoomImagesPath;
                string strFilename;
                IInventoryDAO inventories = new InventoryDAO();

                for (int i = 0; i < _currentImages.Count; i++)
                {
                    strFilename = Path.Combine(path, "pic_" + i.ToString() + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
                    _currentImages[i].Image.Save(strFilename, System.Drawing.Imaging.ImageFormat.Jpeg);

                    if (_currentImages[i].Type != PhotoType.Photo)
                        inventories.SetPhoto((int)_inventoryId, _currentImages[i].Type, strFilename);
                    
                }


                StopVideoCaptureDevice();
                _mainForm.LoadInventory((int)_projectId, false);
                _mainForm.LoadPictures();               
                CloseWindow();
            }
        }

        private void RefreshControlStatus()
        {
            btnAddImagesLeft.Enabled = this._currentImages.Count > 0;
            btnAddImagesRight.Enabled = this._currentImages.Count > 0;
            btnRemoveImagens.Enabled = this._currentImages.Count > 0; 
        }

       
    }
}
