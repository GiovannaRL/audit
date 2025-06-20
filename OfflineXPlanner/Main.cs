using System;
using System.Data;
using System.Windows.Forms;
using OfflineXPlanner.Database;
using OfflineXPlanner.Business;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Security;
using OfflineXPlanner.Utils;
using System.Drawing.Imaging;
using OfflineXPlanner.Domain.Enums;
using System.ComponentModel;
using System.Diagnostics;
using OfflineXPlanner.Domain;

namespace OfflineXPlanner
{
    public partial class Main : Form
    {
        public int? _selectedProjectId;
        List<PictureBox> _selectedRoomPictures = new List<PictureBox>();
        List<PictureBox> _selectedInventoryPictures = new List<PictureBox>();
        private int? _clickedRoomRow;
        private int? _clickedInventoryRow;
        bool _updatingSearchRoom = false;
        bool _updatingSearchDepartment = false;
        int? _lastSelectedRowId = null;
        int? _lastSelectedRowIndex = null;
        string _lastSortedColumnName = null;
        int? _selectedRoomId = null;
        ListSortDirection _lastSortedDirection;        

        public Main()
        {
            InitializeComponent();
        }

        internal string RoomImagesPath
        {
            get
            {
                if (SelectedDepartmentId == null || SelectedRoomId == null)
                    return null;
                var ret = PictureUtil.GetRoomPictureDirectory((int)_selectedProjectId, SelectedDepartmentId??-1, SelectedRoomId??-1);
                if (tabControl.SelectedIndex == 1)
                {
                    if (SelectedInventoryAccessId == null)
                        return null;
                    ret = PictureUtil.GetInventoryPictureDirectory(ret, SelectedInventoryAccessId.GetValueOrDefault());

                }
                if (!Directory.Exists(ret))
                {
                    Directory.CreateDirectory(ret);
                }
                return ret;
            }
        }


        protected int? SelectedDepartmentId
        {
            get
            {
                var grid = tabControl.SelectedIndex == 0 ? gridDepartments : gridInventory;
                if (grid.SelectedRows.Count == 0 && grid.Rows.Count > 0)
                {
                    grid.Rows[0].Selected = true;
                }

                if (grid.SelectedRows.Count == 1)
                {
                    return (int)grid.SelectedRows[0].Cells["department_id"].Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (tabControl.SelectedIndex == 0)
                {
                    return;
                }

                _updatingSearchDepartment = true;
                try
                {
                    var rows = gridDepartments.Rows;
                    var filterDeparmentId = (int)value;
                    for (int i = 0; i < rows.Count; ++i)
                    {
                        var departmentId = (int)rows[i].Cells["department_id"].Value;
                        if (departmentId == filterDeparmentId)
                        {
                            rows[i].Selected = true;
                            break;
                        }
                    }
                }
                finally
                {
                    _updatingSearchDepartment = false;
                }
            }
        }
        protected int? SelectedRoomId
        {
            get
            {
                return _selectedRoomId;
            }
            set
            {

                _updatingSearchRoom = true;
                try
                {
                    _selectedRoomId = value;

                    var rows = gridRooms.Rows;

                    for (int i = 0; i < rows.Count; ++i)
                    {
                        var roomId = (int)rows[i].Cells["room_id"].Value;
                        if (roomId == _selectedRoomId)
                        {
                            rows[i].Selected = true;
                        }
                        else if (rows[i].Selected)
                        {
                            rows[i].Selected = false;
                        }
                    }
                    if (gridRooms.SelectedRows.Count == 0 && gridRooms.Rows.Count > 0)
                    {
                        rows[0].Selected = true;
                        _selectedRoomId = (int) rows[0].Cells["room_id"].Value;
                    }
                }
                finally
                {
                    _updatingSearchRoom = false;
                }
            }
        }


        protected int? SelectedInventoryAccessId
        {
            get
            {

                if (tabControl.SelectedIndex == 0)
                {
                    return null;
                }
                DataGridViewSelectedRowCollection inventory = gridInventory.SelectedRows;
                if (gridInventory.SelectedRows.Count == 1)
                {
                    return (int)gridInventory.SelectedRows[0].Cells["Id"].Value;
                }
                else
                {
                    return null;
                }
            }
        }

        private void openProject_Click(object sender, EventArgs e)
        {
            using (var projectsListForm = new ProjectList(this, false, false))
            {
                if (projectsListForm.ShowDialog() == DialogResult.OK)
                {
                    if (projectsListForm.loadProjectsBtnClick)
                    {
                        importFromAudaxwareToolStripMenuItem_Click(null, null);
                    }
                    else
                    {
                        this._selectedProjectId = projectsListForm.SelectedProject;
                        this.Text = "AudaxWare Offline Editor - " + projectsListForm.SelectedProjectName;
                        this.uploadToAudaxwareToolStripMenuItem.Enabled = true;
                        LoadData();
                    }
                }
            }
        }

        private void HideInventoryColumns()
        {
            gridInventory.Columns["Id"].Visible = false;
            gridInventory.Columns["inventory_id"].Visible = false;
            gridInventory.Columns["project_id"].Visible = false;
            gridInventory.Columns["department_id"].Visible = false;
            gridInventory.Columns["room_id"].Visible = false;
            gridInventory.Columns["Code"].Visible = false;
            gridInventory.Columns["ModelNumber"].Visible = false;
            gridInventory.Columns["JSNNomenclature"].Visible = false;
            gridInventory.Columns["PlannedQty"].Visible = false;
            gridInventory.Columns["Class"].Visible = false;
            gridInventory.Columns["Clin"].Visible = false;
            gridInventory.Columns["UnitBudget"].Visible = false;
            gridInventory.Columns["Resp"].Visible = false;
            gridInventory.Columns["UnitMarkup"].Visible = false;
            gridInventory.Columns["UnitEscalation"].Visible = false;
            gridInventory.Columns["UnitTax"].Visible = false;
            gridInventory.Columns["UnitInstallNet"].Visible = false;
            gridInventory.Columns["UnitInstallMarkup"].Visible = false;
            gridInventory.Columns["UnitOfMeasure"].Visible = false;
            gridInventory.Columns["UnitFreightMarkup"].Visible = false;
            gridInventory.Columns["CadID"].Visible = false;
        }

        private void LoadData()
        {
            if (_selectedProjectId == null)
            {
                MessageBox.Show("Select a project to start.", "Project", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                int selectedProjectID = (int)_selectedProjectId;

                LoadInventory(selectedProjectID);

                // Load data to departments grid
                LoadDepartmentsGridData(selectedProjectID);
                dptsGridView_SelectionChanged(null, null);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }


        void LoadDepartmentsFilter(int projectId)
        {
            IProjectDAO projectDAO = new ProjectDAO();
            var dept = cboDepartmentFilter.SelectedIndex;
            cboDepartmentFilter.DisplayMember = "description";
            cboDepartmentFilter.ValueMember = "department_id";
            cboDepartmentFilter.DataSource = projectDAO.GetDepartments(projectId);
            if (cboDepartmentFilter.Items.Count > dept)
            {
                cboDepartmentFilter.SelectedIndex = dept;
            }

            if (SelectedDepartmentId != null)
            {
                cboDepartmentFilter.SelectedValue = SelectedDepartmentId;
            }
        }

        void LoadRoomsFilter(int projectId)
        {
            IProjectDAO projectDAO = new ProjectDAO();
            var room = cboRoomFilter.SelectedIndex;
            cboRoomFilter.DisplayMember = "Room";
            cboRoomFilter.ValueMember = "room_id";
            cboRoomFilter.DataSource = projectDAO.GetRooms(projectId, cboDepartmentFilter.SelectedValue != null && ((int)cboDepartmentFilter.SelectedValue > 0) ?
                (int?)cboDepartmentFilter.SelectedValue : null);
            if (cboRoomFilter.Items.Count > room)
            {
                cboRoomFilter.SelectedIndex = room;
            }
            if (SelectedRoomId != null)
            {
                cboRoomFilter.SelectedValue = SelectedRoomId;
            }
        }

        public void LoadInventory(int projectId, bool loadFilters = true)
        {
            if (gridInventory.SelectedRows.Count > 0)           
                _lastSelectedRowId = (int)gridInventory.SelectedRows[0].Cells["id"].Value; 
            
            IProjectDAO projectDAO = new ProjectDAO();
            IInventoryDAO inventories = new InventoryDAO();

            gridInventory.DataSource = inventories.GetInventories(projectId);
            HideInventoryColumns();
            SetGridConfiguration();

            if (loadFilters)
            {
                LoadDepartmentsFilter(projectId);
                LoadRoomsFilter(projectId);
            }
            else
            {
                SearchInventory();
            }
            ApplySorting();
            LoadPictures();


            if (_lastSelectedRowId.HasValue)
            {
                foreach (DataGridViewRow row in gridInventory.Rows)
                {
                    if ((int)row.Cells["id"].Value == _lastSelectedRowId.Value)
                    {
                        row.Selected = true;
                        gridInventory.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
        }

        private void GetSorting()
        {
            if (gridInventory.SortedColumn != null)
            {
                _lastSortedColumnName = gridInventory.SortedColumn.Name;
            }
            else
            {
                _lastSortedColumnName = "DateAdded";
            }

            _lastSortedDirection = gridInventory.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
        }
        private void ApplySorting()
        {
            string defaultSortedColumnName = _lastSortedColumnName == null ? "JSN" : _lastSortedColumnName;
            gridInventory.Sort(gridInventory.Columns[defaultSortedColumnName], _lastSortedDirection);
        }

        private void SetGridConfiguration()
        {
            gridInventory.Columns["U1"].Width = 40;
            gridInventory.Columns["U2"].Width = 40;
            gridInventory.Columns["U3"].Width = 40;
            gridInventory.Columns["U4"].Width = 40;
            gridInventory.Columns["U5"].Width = 40;
            gridInventory.Columns["U6"].Width = 40;
            gridInventory.Columns["JSN"].Width = 60;

            foreach (Control ctrl in gridInventory.Controls)
            {
                if (ctrl is VScrollBar vScrollBar)
                {
                    vScrollBar.Width = 300; 
                }
                else if (ctrl is HScrollBar hScrollBar)
                {
                    hScrollBar.Height = 400; 
                }
            }
        }



        private void RotatePictures(PictureBox picture, RotatePictureDirection direction)
        {
                var angle = direction == RotatePictureDirection.Right ? 90 : -90;

                // Rotate the image
                picture.Image = PictureUtil.RotateImage(picture.Image, angle);

                // Save the new Rotate Image
                string path = (picture.Controls[0] as CheckBox).Name;
                picture.Image.Save(path, ImageFormat.Jpeg);
        }

        private Label GetPhotoLabel(string label)
        {
            var lbl = new Label();
            lbl.Location = new Point(0, 159);
            lbl.Text = label;
            return lbl;
        }

        private Button GetRotateButton(string name, RotatePictureDirection angle, Point location)
        {
            Button rotate = new Button();
            rotate.FlatStyle = FlatStyle.Flat;
            rotate.BackColor = Color.Transparent;
            rotate.FlatAppearance.BorderSize = 0;
            rotate.FlatAppearance.MouseDownBackColor = Color.Transparent;
            rotate.FlatAppearance.MouseOverBackColor = Color.Transparent;
            rotate.Name = name;
            rotate.Visible = true;
            rotate.Location = location;
            rotate.Size = new Size(20, 20);
            rotate.Padding = new Padding(0, 0, 0, 6);
            rotate.Cursor = Cursors.Hand;

            if (angle == RotatePictureDirection.Right)
            {
                rotate.Click += new EventHandler(rotateRightBtn_Click);
                rotate.Image = Properties.Resources._90_normal;
            } else 
            {
                rotate.Click += new EventHandler(rotateLeftBtn_Click);
                rotate.Image = Properties.Resources._90_left;
            }

            return rotate;
        }

        public void LoadPictures()
        {
            int x = 0;
            int y = 0;
            int itemsPerLine = 1;
            int spaceBetweenImages = 10;
            var images = GetPictureFileNames();
            panelImages.Controls.Clear();
            panelInventoryImages.Controls.Clear();
            _selectedRoomPictures.Clear();
            _selectedInventoryPictures.Clear();

            foreach (var item in images)
            {
                PictureBox pb = new PictureBox();
                pb.Size = new System.Drawing.Size(150, 170);
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Location = new Point(x, y);
                pb.Click += new EventHandler(OpenImage_click);
                pb.Tag = item;
                CheckBox chk = new CheckBox();
                chk.Size = new System.Drawing.Size(30, 30);
                chk.Padding = new Padding(0, 0, 0, 5);
                chk.BackColor = Color.Transparent;
                chk.Name = item;
                chk.CheckedChanged += new EventHandler(PictureCheckboxChanged);
                pb.Controls.Add(chk);

                // Rotate Right Button
                pb.Controls.Add(GetRotateButton(item, RotatePictureDirection.Right, new Point(130, 0)));

                // Rotate Left Button
                pb.Controls.Add(GetRotateButton(item, RotatePictureDirection.Left, new Point(110, 0)));

                if (itemsPerLine < 2)
                {
                    x += 200 + spaceBetweenImages;
                    itemsPerLine++;
                }
                else
                {
                    y += 180 + spaceBetweenImages;
                    x = 0;
                    itemsPerLine = 1;
                }

                if (tabControl.SelectedIndex == 1)
                {
                    DataGridViewSelectedRowCollection inventory = gridInventory.SelectedRows;
                    if (inventory.Count == 1)
                    {
                        var file = inventory[0].Cells["PhotoFile"].Value as string;
                        if (string.Compare(item, file, true) == 0)
                        {
                            pb.Controls.Add(GetPhotoLabel("Photo", new Point(0, pb.Height - 20)));
                        }
                        else
                        {
                            file = inventory[0].Cells["TagPhotoFile"].Value as string;
                            if (string.Compare(item, inventory[0].Cells["TagPhotoFile"].Value.ToString(), true) == 0)
                            {
                                pb.Controls.Add(GetPhotoLabel("Tag", new Point(0, pb.Height - 20)));
                            }
                        }
                    }
                }
                else
                {
                    DataGridViewSelectedRowCollection room = gridRooms.SelectedRows;
                    if (room.Count == 1)
                    {
                        var file = room[0].Cells["PhotoFile"].Value as string;
                        if (string.Compare(item, file, true) == 0)
                        {
                            pb.Controls.Add(GetPhotoLabel("Photo", new Point(0, pb.Height - 20))); // Rótulo próximo à parte inferior da imagem
                        }
                    }
                }

                pb.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(item)));

                if (tabControl.SelectedIndex == 0)
                    panelImages.Controls.Add(pb);
                else
                    panelInventoryImages.Controls.Add(pb);
            }
        }

        // Método GetPhotoLabel atualizado para incluir a posição
        private Label GetPhotoLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = location
            };
        }


        private void OpenImage_click(object sender, EventArgs e)
        {
            if (sender is PictureBox pb)
            {
                string imagePath = pb.Tag.ToString();
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = imagePath,
                    UseShellExecute = true
                };
                Process.Start(psi);          

            }      
        }

        private void PictureCheckboxChanged(object sender, EventArgs e)
        {
            var isRoom = false;
            if (tabControl.SelectedIndex == 0)
                isRoom = true;

            var chk = ((CheckBox)sender);
            
            if (isRoom)
            {
                var picture = panelImages.Controls.OfType<PictureBox>()
                    .FirstOrDefault(p => p.Controls.OfType<CheckBox>().Contains(chk));

                if (chk.Checked)
                    this._selectedRoomPictures.Add(picture);
                else
                    this._selectedRoomPictures.Remove(picture);
            } else
            {
                var picture = panelInventoryImages.Controls.OfType<PictureBox>()
                    .FirstOrDefault(p => p.Controls.OfType<CheckBox>().Contains(chk));

                if (chk.Checked)
                    this._selectedInventoryPictures.Add(picture);
                else
                    this._selectedInventoryPictures.Remove(picture);
            }
        }

        private List<string> GetPictureFileNames()
        {
            var path = RoomImagesPath;
            if (path == null)
                return new List<string>();
            return Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png"))
                .ToList();
        }

        public void LoadDepartmentsGridData(int project_id)
        {
            this.gridDepartments.DataSource = DepartmentBusiness.GetDepartments(project_id);
            // hide extras columns
            this.gridDepartments.Columns["project_id"].Visible = false;
            this.gridDepartments.Columns["department_id"].Visible = false;
            LoadDepartmentsFilter(project_id);
        }

        private void LoadRoomsGridData()
        {
            int? lastSelectedRoomId = SelectedRoomId;
            var rooms = RoomBusiness.Get((int)_selectedProjectId, SelectedDepartmentId != null ? (int)SelectedDepartmentId : -1);
            this.gridRooms.DataSource = rooms;
            // hide extras columns
            this.gridRooms.Columns["project_id"].Visible = false;
            this.gridRooms.Columns["department_id"].Visible = false;
            this.gridRooms.Columns["room_id"].Visible = false;
            this.gridRooms.Columns["room"].Visible = false;
            LoadRoomsFilter((int)_selectedProjectId);
            SelectedRoomId = lastSelectedRoomId;
            LoadPictures();
        }

        private void LoadEditInventory(object sender, EventArgs e)
        {
            //var form = new EditInventory();
            //form.Show();
            //this.Hide();
        }

        private void SearchInventoryGrid(object sender, KeyEventArgs e)
        {
            SearchInventory();

        }

        private void InventoryList_Load(object sender, EventArgs e)
        {
            try
            {
                IOUtil.MoveDatabaseFile();

                cboDepartmentFilter.DisplayMember = "department";
                cboDepartmentFilter.ValueMember = "department";
                cboRoomFilter.DisplayMember = "room";
                cboRoomFilter.ValueMember = "room";

                //cboEditDepartment.DisplayMember = "department";
                //cboEditDepartment.ValueMember = "department";
                //cboEditRoom.DisplayMember = "room";
                //cboEditRoom.ValueMember = "room";

                //m_iDesignWidth = this.Width;
                //if (m_CameraManager.GetCameraNames() != null)
                //{
                //    foreach (string temp in m_CameraManager.GetCameraNames())
                //    {
                //        cbCameraSelect.Items.Add(temp);
                //    }

                //    cbCameraSelect.SelectedIndexChanged += cbCameraSelect_SelectedIndexChanged;
                //}

                if (_selectedProjectId != null)
                {
                    LoadData();
                }
                else
                {
                    openProject_Click(null, null);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DuplicateInventory(int inventoryId)
        {
            int departmentId = cboDepartmentFilter.SelectedValue != null ? (int)cboDepartmentFilter.SelectedValue : -1;
            int roomId = cboRoomFilter.SelectedValue != null ? (int)cboRoomFilter.SelectedValue : -1;

            using (SelectDuplicateQtyForm selectQtyForm = new SelectDuplicateQtyForm(
                _selectedProjectId ?? -1, departmentId, roomId, inventoryId))
            {
                if (selectQtyForm.ShowDialog() == DialogResult.OK)
                {
                    var newAssetId = selectQtyForm._newAssetId;
                    LoadInventory((int)_selectedProjectId, false);

                    if (newAssetId != null)
                        SelectAssetById((int)newAssetId);

                }
            }
        }

        private void btnDuplicateAsset_Click(object sender, EventArgs e)
        {
            var selected = gridInventory.SelectedRows;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select an asset first", "Clone", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int id = (int)selected[0].Cells["Id"].Value;
            GetSorting();
            DuplicateInventory(id);

        }
        private void duplicateInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (_clickedInventoryRow == null || _clickedInventoryRow.GetValueOrDefault() < 0)
            {
                MessageBox.Show("You need to right click on a item", "Duplicate Room", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int id = (int)gridInventory.Rows[this._clickedInventoryRow.GetValueOrDefault()].Cells["Id"].Value;
            DuplicateInventory(id);
        }

        void DuplicateRoom(DataGridViewRow row)
        {

            int roomId = (int)row.Cells["room_id"].Value;
            string roomName = row.Cells["room_name"].Value.ToString();
            string roomNumber = row.Cells["room_number"].Value.ToString();
            Cursor.Current = Cursors.WaitCursor;

            var form = new AddEditRoom((int)_selectedProjectId, (int)SelectedDepartmentId, roomId, roomNumber, roomName);
            DialogResult result = form.ShowDialog();
            if (result == DialogResult.Abort)
            {
                MessageBox.Show("Error to try duplicate the room", "Duplicate Room", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (result == DialogResult.OK)
            {
                int? newRoomId = form._newRoomId;
                LoadRoomsGridData();
                LoadInventory(_selectedProjectId.GetValueOrDefault());

                if (newRoomId > 0)
                    SelectRoomById((int)newRoomId);             
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnDuplicateRoom_Click(object sender, EventArgs e)
        {
            DataGridViewCellEventArgs e1 = null;
            try
            {
                e1 = (DataGridViewCellEventArgs)e;
            }
            catch (Exception) { }

            if (e1 == null || e1.RowIndex >= 0)
            {
                DuplicateRoom(gridRooms.SelectedRows[0]);
            }

        }

        private void duplicateRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_clickedRoomRow == null || _clickedRoomRow.GetValueOrDefault() < 0)
            {
                MessageBox.Show("You need to right click on a room", "Duplicate Room", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DuplicateRoom(gridRooms.Rows[this._clickedRoomRow.GetValueOrDefault()]);
        }

        private void gridInventory_CellMouseDownClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                switch (e.Button)
                {
                    case MouseButtons.Right:
                        this._clickedInventoryRow = e.RowIndex;
                        gridInventory.ContextMenuStrip.Show(gridInventory, new Point(e.X, e.Y));
                        break;
                }
            }
            else
            {
                this._clickedInventoryRow = null;
            }
        }

        private void gridRooms_CellMouseDownClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                switch (e.Button)
                {
                    case MouseButtons.Right:
                        this._clickedRoomRow = e.RowIndex;
                        gridRooms.ContextMenuStrip.Show(gridRooms, new Point(e.X, e.Y));
                        break;
                }
            }
            else
            {
                this._clickedRoomRow = null;
            }
        }

        private void cbPhase_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchInventory();
        }
        private void cboInventoryPhotoState_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchInventory();
        }

        private void SearchInventory()
        {
            if (txtSearch.Text.Trim() == "" && cboDepartmentFilter.Text == "" && cboRoomFilter.Text == "" && cboInventoryPhotoState.Text == "")
            {
                (gridInventory.DataSource as DataTable).DefaultView.RowFilter = "";
            }
            else
            {
                string rowFilter = "";
                if (txtSearch.Text.Trim() != "")
                {
                    rowFilter = string.Format("(Phase like '%{0}%' or Department like '%{0}%' or RoomName like '%{0}%' or RoomNumber like '%{0}%' or Code like '%{0}%' or ModelNumber like '%{0}%' or ModelName like '%{0}%' or JSN like '%{0}%' or Manufacturer like '%{0}%' or Description like '%{0}%')", txtSearch.Text.Trim().Replace("'", "''"));
                }
                if (cboDepartmentFilter.SelectedIndex > 0)
                {
                    if (rowFilter != "")
                        rowFilter += " and ";
                    rowFilter += string.Format("Department = '{0}'", cboDepartmentFilter.Text.Replace("'", "''"));
                }
                if (cboRoomFilter.SelectedIndex > 0)
                {
                    if (rowFilter != "")
                        rowFilter += " and ";
                    rowFilter += string.Format("((RoomName + ' -- ' + RoomNumber)  = '{0}')", cboRoomFilter.Text.Replace("'", "''"));
                }
                if (cboInventoryPhotoState.SelectedIndex > 0)
                {
                    if (rowFilter != "")
                        rowFilter += " and ";
                    switch(cboInventoryPhotoState.SelectedIndex)
                    {
                        case 1:
                            rowFilter += "(IsNull(PhotoFile,'')  =  '')";
                            break;
                        case 2:
                            rowFilter += "(IsNull(TagPhotoFile,'')  =  '')";
                            break;
                        case 3:
                            rowFilter += "((IsNull(TagPhotoFile,'')  =  '')";
                            rowFilter += " OR (IsNull(TagPhotoFile,'')  =  ''))";
                            break;
                    }
                }
                (gridInventory.DataSource as DataTable).DefaultView.RowFilter = rowFilter;
            }
            LoadPictures();
        }

        private void cbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchInventory();
            if (cboDepartmentFilter.SelectedValue != null)
            {
                SelectedDepartmentId = (int)cboDepartmentFilter.SelectedValue;
            }
            LoadRoomsFilter(_selectedProjectId != null ? (int)_selectedProjectId : -1);
        }

        private void cbRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRoomFilter.SelectedValue != null)
            {
                SelectedRoomId = (int)cboRoomFilter.SelectedValue;
            }
            SearchInventory();
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void btnAddDepartment_Click(object sender, EventArgs e)
        {
            if (_selectedProjectId == null)
            {
                MessageBox.Show("Select a project to add a new department.", "Department", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SelectedRoomId = -1;
            var form = new AddEditDepartment((int)_selectedProjectId);
            form.ShowDialog();
            var newDepartmentId = form._newDepartmentId;
            LoadDepartmentsGridData((int)_selectedProjectId);

            if (newDepartmentId > 0)
            {
                foreach (DataGridViewRow row in gridDepartments.Rows)
                {
                    if ((int)row.Cells["department_id"].Value == newDepartmentId.Value)
                    {
                        row.Selected = true; 
                        gridDepartments.FirstDisplayedScrollingRowIndex = row.Index; 
                        break;
                    }
                }
            }

        }

        private void btnEditDepartment_Click(object sender, EventArgs e)
        {
            if (_selectedProjectId == null)
            {
                MessageBox.Show("Select a project to edit department.", "Department", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataGridViewSelectedRowCollection department = gridDepartments.SelectedRows;

            if (department.Count == 0)
            {
                MessageBox.Show("Please select a department first.", "Department", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataGridViewCellEventArgs e1 = null;
            try
            {
                e1 = (DataGridViewCellEventArgs)e;
            }
            catch (Exception) { }

            if (e1 == null || e1.RowIndex >= 0)
            {
                var form = new AddEditDepartment((int)_selectedProjectId, department);
                if (form.ShowDialog() != DialogResult.Cancel)
                {
                    LoadDepartmentsGridData((int)_selectedProjectId);
                }
            }
        }

        private void btnAddRoom_Click(object sender, EventArgs e)
        {
            if (SelectedDepartmentId == null)
            {
                MessageBox.Show("Please select a department first.", "Department", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            var form = new AddEditRoom((int)_selectedProjectId, (int)SelectedDepartmentId);
            form.ShowDialog();
            var newRoomId = form._newRoomId;
            LoadRoomsGridData();

            if (newRoomId > 0)
                SelectRoomById((int)newRoomId);


        }

        private void btnEditRoom_Click(object sender, EventArgs e)
        {
            DataGridViewCellEventArgs e1 = null;
            try
            {
                e1 = (DataGridViewCellEventArgs)e;
            }
            catch (Exception) { }

            if (e1 == null || e1.RowIndex >= 0)
            {
                DataGridViewSelectedRowCollection room = gridRooms.SelectedRows;

                var form = new AddEditRoom((int)_selectedProjectId, (int)SelectedDepartmentId, room);
                if (form.ShowDialog() != DialogResult.Cancel)
                {
                    LoadRoomsGridData();
                    LoadInventory((int)_selectedProjectId);
                }
            }
        }

        private async void importFromAudaxwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SecurityUtil.IsLogged())
            {
                importProjects();
            }
            else
            {
                using (Login loginForm = new Login(true))
                {
                    // Open login form
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // If login was successfully done import projects and go to projects list page
                        importProjects();
                    }
                }
            }
        }

        private void importProjects()
        {

            using (var loadProjectDataForm = new LoadProjectDataForm())
            {
                if (loadProjectDataForm.ShowDialog() == DialogResult.OK)
                {
                    this._selectedProjectId = loadProjectDataForm.SelectedProject;
                    this.uploadToAudaxwareToolStripMenuItem.Enabled = true;
                    LoadData();
                }
            }
        }

        private bool ExportData(int project_id)
        {
            // Export data
            Cursor.Current = Cursors.WaitCursor;
            bool response = ExportDataUtil.ExportData(project_id);
            Cursor.Current = Cursors.Default;

            return response;
        }

        private void uploadToAudaxwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this._selectedProjectId == null)
            {
                MessageBox.Show("You need to have an opened project to upload", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (SecurityUtil.IsLogged())
            {
                if (ExportData(this._selectedProjectId.GetValueOrDefault()))
                {
                    MessageBox.Show("Uploaded has completed sucessfully", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                using (Login loginForm = new Login())
                {
                    // Open login form
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        if (ExportData(this._selectedProjectId.GetValueOrDefault()))
                        {
                            MessageBox.Show("Uploaded has completed sucessfully", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void dptsGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (!_updatingSearchDepartment)
            {
                SelectedRoomId = -1;
                LoadRoomsGridData();
                if (SelectedDepartmentId != null)
                    cboDepartmentFilter.SelectedValue = SelectedDepartmentId;
            }
        }

        private void btnAddImage_Click(object sender, EventArgs e)
        {
            if (SelectedRoomId == null)
            {
                MessageBox.Show("Please select a room first.", "Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            var form = new AddPicture(this, _selectedProjectId ?? 0, (int)SelectedDepartmentId, (int)SelectedRoomId);
            form.ShowDialog();
        }

        private void btnAddAsset_Click(object sender, EventArgs e)
        {
            var totalRows = gridInventory.RowCount;
            int departmentId = 0;
            int roomId = 0;
            if (cboDepartmentFilter.SelectedValue != null)
                departmentId = (int)cboDepartmentFilter.SelectedValue;
            if (cboRoomFilter.SelectedValue != null)
                roomId = (int)cboRoomFilter.SelectedValue;
            var editInventoryForm = new EditInventory((int)_selectedProjectId, departmentId, roomId);
            GetSorting();
            editInventoryForm.ShowDialog();
            var newAssetId = editInventoryForm._newAssetId;

            LoadInventory((int)_selectedProjectId, false);

            if (newAssetId != null)
                SelectAssetById((int)newAssetId);           

        }

        private void roomsGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (!_updatingSearchRoom)
            {
                if (gridRooms.SelectedRows.Count == 1)
                {
                    SelectedRoomId = (int)gridRooms.SelectedRows[0].Cells["room_id"].Value;
                }
                else
                {
                    SelectedRoomId = -1;
                }
                LoadPictures();
            }
        }

        private void roomsGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            LoadPictures();
        }

        private void btnAddPictureInventory_Click(object sender, EventArgs e)
        {
            if (SelectedInventoryAccessId == null)
            {
                MessageBox.Show("Please select an asset first.", "Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            var form = new AddPicture(this, (int)_selectedProjectId, (int)SelectedDepartmentId, (int)SelectedRoomId, SelectedInventoryAccessId);
            form.ShowDialog();
        }

        private void btnSaveAsset_Click(object sender, EventArgs e)
        {
            var selected = gridInventory.SelectedRows;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select an asset first", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataGridViewCellEventArgs e1 = null;
            try
            {
                e1 = e as DataGridViewCellEventArgs;
            }
            catch (Exception) { }

            if (e1 == null || e1.RowIndex >= 0)
            {
                var editInventoryForm = new EditInventory((int)_selectedProjectId, selected[0]);
                GetSorting();
                if (editInventoryForm.ShowDialog() != DialogResult.Cancel)
                {
                    LoadInventory((int)_selectedProjectId, false);
                }
            }

        }

        private void gridInventory_SelectionChanged(object sender, EventArgs e)
        {
            LoadPictures();
        }
        private void btnBrowsePicture_Click(object sender, EventArgs e)
        {
            if (SelectedRoomId == null)
            {
                MessageBox.Show("Please select a room first", "Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select a file";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "Image files (*.jpg, *.png) | *.jpg; *png";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            DialogResult result = fdlg.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fdlg.FileName;
                string strFilename = Path.Combine(RoomImagesPath, "pic_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(file));
                File.Copy(file, strFilename);
                LoadPictures();
            }
        }

        private void btnBrowseInventory_Click(object sender, EventArgs e)
        {

            if (SelectedInventoryAccessId == null)
            {
                MessageBox.Show("Please select an asset first.", "Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select a file";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "Image files (*.jpg, *.png) | *.jpg; *.png";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            DialogResult result = fdlg.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fdlg.FileName;
                string strFilename = Path.Combine(RoomImagesPath, "pic_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + Path.GetExtension(file));
                File.Copy(file, strFilename);
                LoadPictures();
            }
        }

        private void btnDelDepartment_Click(object sender, EventArgs e)
        {
            if (SelectedDepartmentId == null)
            {
                MessageBox.Show("Select a department first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IDepartmentDAO department = new DepartmentDAO();
                department.DeleteDepartment((int)_selectedProjectId, (int)SelectedDepartmentId);
                try
                {
                    var departmentPath = this.RoomImagesPath.Split(new string[] { "room" }, StringSplitOptions.None)[0];
                    movePictureWhenDelete(departmentPath);
                    Directory.Delete(departmentPath, true);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error trying to delete department pictures. Please contact support");
                }

                LoadDepartmentsGridData((int)_selectedProjectId);
            }
        }

        private void btnDelRoom_Click(object sender, EventArgs e)
        {
            if (SelectedRoomId == null)
            {
                MessageBox.Show("Select a room first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IRoomDAO room = new RoomDAO();
                room.DeleteRoom((int)_selectedProjectId, (int)SelectedDepartmentId, (int)SelectedRoomId);
                try
                {
                    movePictureWhenDelete(this.RoomImagesPath);
                    Directory.Delete(this.RoomImagesPath, true);
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error trying to delete room pictures. Please contact support");
                }

                LoadRoomsGridData();
                LoadInventory((int)_selectedProjectId, false);
            }
        }

        private void loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!SecurityUtil.IsLogged())
            {
                using (Login loginForm = new Login(true))
                {
                    // Open login form
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                }
            }

            CatalogBusiness.importCatalogData();
        }

        private void btnDeleteAsset_Click(object sender, EventArgs e)
        {
            var selected = gridInventory.SelectedRows;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select an asset first", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((int)selected[0].Cells["inventory_id"].Value > 0)
            {
                MessageBox.Show("This asset has already been uploaded and cannot be deleted.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                InventoryBusiness.Delete((int)selected[0].Cells["Id"].Value);
                try
                {
                    Directory.Delete(this.RoomImagesPath, true);
                }
                catch (Exception)
                {

                    throw;
                }

                LoadInventory((int)_selectedProjectId);
            }
        }

        private void btnDelPictureInventory_Click(object sender, EventArgs e)
        {
            IInventoryDAO inventories = new InventoryDAO();
            var selected = gridInventory.SelectedRows;
            if (_selectedInventoryPictures.Count == 0)
            {
                MessageBox.Show("Select at least one picture to delete.", "Inventory Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var picturesToDelete = new List<PictureBox>(_selectedInventoryPictures);

                foreach (var pictureBox in picturesToDelete)
                {
                    var pictureCheckBox = pictureBox.Controls.OfType<CheckBox>().FirstOrDefault();
                    var picturePath = pictureCheckBox.Name;

                    movePictureWhenDelete(picturePath);
                    if (selected[0].Cells["Id"].Value != null)
                        UpdateInventoryPhotoFields((int)selected[0].Cells["Id"].Value, picturePath);
                    _selectedInventoryPictures.Remove(pictureBox);
                }
                LoadPictures();
                gridInventory.Refresh();
                if (_selectedProjectId.HasValue)
                {
                    LoadInventory(_selectedProjectId.Value, false);
                }
            }
        }

        public void UpdateInventoryPhotoFields(int id, string picturePath)
        {
            IInventoryDAO inventories = new InventoryDAO();
            Inventory inventory = inventories.GetInventoryItem((int)_selectedProjectId, id);
            if (inventory.PhotoFile == picturePath)
            {
                inventories.UpdatePhotoFields(id, PhotoType.Asset, picturePath);
            }

            if (inventory.TagPhotoFile == picturePath)
            {
                inventories.UpdatePhotoFields(id, PhotoType.Tag, picturePath);
            }
        }

        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            if (_selectedRoomPictures.Count == 0)
            {
                MessageBox.Show("Select at least one picture to delete.", "Room Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                while (_selectedRoomPictures.Count > 0)
                {
                    movePictureWhenDelete(_selectedRoomPictures[0].Controls.OfType<CheckBox>().FirstOrDefault().Name);
                    _selectedRoomPictures.Remove(_selectedRoomPictures[0]);
                }
                LoadPictures();
            }
        }

        private void movePictureWhenDelete(string picturePath) {

            if (File.Exists(picturePath))
            {
                string fileName = Path.GetFileName(picturePath);
                string onlyPath = picturePath.Replace("\\" + fileName, "");
                string destinationFilePath = PictureUtil.GetDeleteItemsDirectory(onlyPath);

                if (!Directory.Exists(destinationFilePath))
                    Directory.CreateDirectory(destinationFilePath);

                File.Move(picturePath, Path.Combine(destinationFilePath, fileName));

            }
            else
            {
                movePicture(picturePath);
                searchData(picturePath);
            }
        }

        private void searchData(string picturePath) {
            string[] subDirectories = Directory.GetDirectories(picturePath);
            foreach (var sub in subDirectories)
            {
                movePicture(sub);
                searchData(sub);
            }
        }

        private void movePicture(string picturePath) {
            var destinationFilePath = PictureUtil.GetDeleteItemsDirectory(picturePath);
            if (!Directory.Exists(destinationFilePath))
                Directory.CreateDirectory(destinationFilePath);

            var allFiles = Directory.GetFiles(picturePath);
            foreach (var item in allFiles)
            {
                File.Move(item, Path.Combine(destinationFilePath, Path.GetFileName(item)));
            }
        }


        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cboDepartmentFilter.SelectedIndex = -1;
            cboRoomFilter.SelectedIndex = -1;
            cboInventoryPhotoState.SelectedIndex = -1;
            LoadInventory((int)_selectedProjectId);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.tabControl.SelectedIndex == 0)
            {
                LoadRoomsGridData();
            }
            else
            {
                cboRoomFilter.SelectedValue = SelectedRoomId;
                LoadPictures();
            }
        }

        private PictureBox GetPictureFromRotatebutton(Button button)
        {
            PictureBox picture = panelImages.Controls.OfType<PictureBox>()
                    .FirstOrDefault(p => p.Controls.OfType<Button>().Contains(button));

            if (picture == null)
            {
                picture = panelInventoryImages.Controls.OfType<PictureBox>()
                    .FirstOrDefault(p => p.Controls.OfType<Button>().Contains(button));
            }

            return picture;
        }

        private void rotateLeftBtn_Click(object sender, EventArgs e)
        {
            this.RotatePictures(GetPictureFromRotatebutton(sender as Button), RotatePictureDirection.Left);
        }

        private void rotateRightBtn_Click(object sender, EventArgs e)
        {
            this.RotatePictures(GetPictureFromRotatebutton(sender as Button), RotatePictureDirection.Right);
        }


        void SetAsInventoryPicture(PhotoType type)
        {
            if (_selectedInventoryPictures.Count == 0)
            {
                MessageBox.Show("Select one picture", "Set Photo Type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (_selectedInventoryPictures.Count > 1)
            {
                MessageBox.Show("You must select only one picture", "Set Photo Type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            IInventoryDAO inventories = new InventoryDAO();
            var selected = gridInventory.SelectedRows;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select an asset first", "Set Photo Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (inventories.SetPhoto((int)selected[0].Cells["Id"].Value, type,
                _selectedInventoryPictures[0].Controls.OfType<CheckBox>().FirstOrDefault().Name))
            {
                if (type == PhotoType.Asset)
                    selected[0].Cells["PhotoFile"].Value = _selectedInventoryPictures[0].Controls.OfType<CheckBox>().FirstOrDefault().Name;
                else
                    selected[0].Cells["TagPhotoFile"].Value = _selectedInventoryPictures[0].Controls.OfType<CheckBox>().FirstOrDefault().Name;
            }
            LoadPictures();
        }

        private void btnSetAsAssetPhoto_Click(object sender, EventArgs e)
        {
            SetAsInventoryPicture(PhotoType.Asset);
        }

        private void btnSetAsTag_Click(object sender, EventArgs e)
        {
            SetAsInventoryPicture(PhotoType.Tag);
        }

        private void DeptRoomsTab_Click(object sender, EventArgs e)
        {

        }

        private void btnSetAsPhoto_Click(object sender, EventArgs e)
        {
            if (_selectedRoomPictures.Count == 0)
            {
                MessageBox.Show("Select one picture", "Set Photo Type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (_selectedRoomPictures.Count > 1)
            {
                MessageBox.Show("You must select only one picture", "Set Photo Type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var selected = gridRooms.SelectedRows;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select a room first", "Set Photo Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IRoomDAO roomDAO = new RoomDAO();
            var project_id = Convert.ToInt32(selected[0].Cells["project_id"].Value);
            var department_id = Convert.ToInt32(selected[0].Cells["department_id"].Value);
            var room_id = Convert.ToInt32(selected[0].Cells["room_id"].Value);

            if (roomDAO.SetPhoto(project_id, department_id, room_id,
                _selectedRoomPictures[0].Controls.OfType<CheckBox>().FirstOrDefault().Name))
            {
                selected[0].Cells["PhotoFile"].Value = _selectedRoomPictures[0].Controls.OfType<CheckBox>().FirstOrDefault().Name;
            }
            LoadPictures();

        }

        public void SelectRoomById(int newRoomId)
        {
            foreach (DataGridViewRow item in gridRooms.Rows)
            {
                if ((int)item.Cells["room_id"].Value == newRoomId)
                {
                    item.Selected = true;
                    gridRooms.FirstDisplayedScrollingRowIndex = item.Index;
                    break;
                }
            }
        }

        public void SelectAssetById(int newAssetId)
        {
            foreach (DataGridViewRow row in gridInventory.Rows)
            {
                if ((int)row.Cells["Id"].Value == newAssetId)
                {
                    row.Selected = true;
                    gridInventory.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }




    }
}
