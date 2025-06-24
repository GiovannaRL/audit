using System.Windows.Forms;

namespace OfflineXPlanner
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.project_name = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromAudaxwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadToAudaxwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tabControl = new System.Windows.Forms.TabControl();
            this.DeptRoomsTab = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnSetAsPhoto = new System.Windows.Forms.Button();
            this.btnBrowsePicture = new System.Windows.Forms.Button();
            this.panelImages = new System.Windows.Forms.Panel();
            this.btnDeleteImage = new System.Windows.Forms.Button();
            this.btnAddImage = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnDuplicateRoom = new System.Windows.Forms.Button();
            this.btnEditRoom = new System.Windows.Forms.Button();
            this.btnDelRoom = new System.Windows.Forms.Button();
            this.btnAddRoom = new System.Windows.Forms.Button();
            this.gridRooms = new System.Windows.Forms.DataGridView();
            this.room_number = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhotoFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.room_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.roomItemCtxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.duplicateRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnEditDepartment = new System.Windows.Forms.Button();
            this.btnDelDepartment = new System.Windows.Forms.Button();
            this.btnAddDepartment = new System.Windows.Forms.Button();
            this.gridDepartments = new System.Windows.Forms.DataGridView();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ContentsTab = new System.Windows.Forms.TabPage();
            this.assetInventoryGrpBox = new System.Windows.Forms.GroupBox();
            this.btnDuplicateAsset = new System.Windows.Forms.Button();
            this.cboInventoryPhotoState = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.gridInventory = new System.Windows.Forms.DataGridView();
            this.inventoryItemCtxmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.duplicateInventoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSaveAsset = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnDeleteAsset = new System.Windows.Forms.Button();
            this.cboDepartmentFilter = new System.Windows.Forms.ComboBox();
            this.btnAddAsset = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cboRoomFilter = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSetAsTagInventory = new System.Windows.Forms.Button();
            this.btnSetAsPhotoInventory = new System.Windows.Forms.Button();
            this.btnBrowseInventory = new System.Windows.Forms.Button();
            this.panelInventoryImages = new System.Windows.Forms.Panel();
            this.btnDelPictureInventory = new System.Windows.Forms.Button();
            this.btnAddPictureInventory = new System.Windows.Forms.Button();
            this.audaxware_offlineDataSet = new OfflineXPlanner.audaxware_offlineDataSet();
            this.audaxwareofflineDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.DeptRoomsTab.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRooms)).BeginInit();
            this.roomItemCtxMenu.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDepartments)).BeginInit();
            this.ContentsTab.SuspendLayout();
            this.assetInventoryGrpBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridInventory)).BeginInit();
            this.inventoryItemCtxmenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.audaxware_offlineDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.audaxwareofflineDataSetBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // project_name
            // 
            this.project_name.AutoSize = true;
            this.project_name.Location = new System.Drawing.Point(68, 31);
            this.project_name.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.project_name.Name = "project_name";
            this.project_name.Size = new System.Drawing.Size(0, 13);
            this.project_name.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1401, 24);
            this.menuStrip1.TabIndex = 27;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.importFromAudaxwareToolStripMenuItem,
            this.uploadToAudaxwareToolStripMenuItem,
            this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(356, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openProject_Click);
            // 
            // importFromAudaxwareToolStripMenuItem
            // 
            this.importFromAudaxwareToolStripMenuItem.Name = "importFromAudaxwareToolStripMenuItem";
            this.importFromAudaxwareToolStripMenuItem.Size = new System.Drawing.Size(356, 22);
            this.importFromAudaxwareToolStripMenuItem.Text = "Load project from Audaxware...";
            this.importFromAudaxwareToolStripMenuItem.Click += new System.EventHandler(this.importFromAudaxwareToolStripMenuItem_Click);
            // 
            // uploadToAudaxwareToolStripMenuItem
            // 
            this.uploadToAudaxwareToolStripMenuItem.Enabled = false;
            this.uploadToAudaxwareToolStripMenuItem.Name = "uploadToAudaxwareToolStripMenuItem";
            this.uploadToAudaxwareToolStripMenuItem.Size = new System.Drawing.Size(356, 22);
            this.uploadToAudaxwareToolStripMenuItem.Text = "Upload project to Audaxware...";
            this.uploadToAudaxwareToolStripMenuItem.Click += new System.EventHandler(this.uploadToAudaxwareToolStripMenuItem_Click);
            // 
            // loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem
            // 
            this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem.Name = "loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem";
            this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem.Size = new System.Drawing.Size(356, 22);
            this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem.Text = "Load Manufacturers / JSN Catalog from AudaxWare...";
            this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem.Click += new System.EventHandler(this.loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.DeptRoomsTab);
            this.tabControl.Controls.Add(this.ContentsTab);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(7, 31);
            this.tabControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(2301, 736);
            this.tabControl.TabIndex = 28;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // DeptRoomsTab
            // 
            this.DeptRoomsTab.Controls.Add(this.groupBox5);
            this.DeptRoomsTab.Controls.Add(this.groupBox4);
            this.DeptRoomsTab.Controls.Add(this.groupBox2);
            this.DeptRoomsTab.Location = new System.Drawing.Point(4, 29);
            this.DeptRoomsTab.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DeptRoomsTab.Name = "DeptRoomsTab";
            this.DeptRoomsTab.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DeptRoomsTab.Size = new System.Drawing.Size(2293, 703);
            this.DeptRoomsTab.TabIndex = 0;
            this.DeptRoomsTab.Text = "Dept & Rooms";
            this.DeptRoomsTab.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.btnSetAsPhoto);
            this.groupBox5.Controls.Add(this.btnBrowsePicture);
            this.groupBox5.Controls.Add(this.panelImages);
            this.groupBox5.Controls.Add(this.btnDeleteImage);
            this.groupBox5.Controls.Add(this.btnAddImage);
            this.groupBox5.Location = new System.Drawing.Point(868, 11);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox5.Size = new System.Drawing.Size(459, 478);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Room Images";
            // 
            // btnSetAsPhoto
            // 
            this.btnSetAsPhoto.Location = new System.Drawing.Point(249, 23);
            this.btnSetAsPhoto.Margin = new System.Windows.Forms.Padding(1);
            this.btnSetAsPhoto.Name = "btnSetAsPhoto";
            this.btnSetAsPhoto.Size = new System.Drawing.Size(81, 73);
            this.btnSetAsPhoto.TabIndex = 10;
            this.btnSetAsPhoto.Text = "Set as Photo";
            this.btnSetAsPhoto.UseVisualStyleBackColor = true;
            this.btnSetAsPhoto.Click += new System.EventHandler(this.btnSetAsPhoto_Click);
            // 
            // btnBrowsePicture
            // 
            this.btnBrowsePicture.Location = new System.Drawing.Point(6, 23);
            this.btnBrowsePicture.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnBrowsePicture.Name = "btnBrowsePicture";
            this.btnBrowsePicture.Size = new System.Drawing.Size(81, 73);
            this.btnBrowsePicture.TabIndex = 5;
            this.btnBrowsePicture.Text = "Browse";
            this.btnBrowsePicture.UseVisualStyleBackColor = true;
            this.btnBrowsePicture.Click += new System.EventHandler(this.btnBrowsePicture_Click);
            // 
            // panelImages
            // 
            this.panelImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelImages.AutoScroll = true;
            this.panelImages.Location = new System.Drawing.Point(6, 101);
            this.panelImages.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panelImages.Name = "panelImages";
            this.panelImages.Size = new System.Drawing.Size(449, 373);
            this.panelImages.TabIndex = 4;
            // 
            // btnDeleteImage
            // 
            this.btnDeleteImage.Location = new System.Drawing.Point(168, 23);
            this.btnDeleteImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDeleteImage.Name = "btnDeleteImage";
            this.btnDeleteImage.Size = new System.Drawing.Size(81, 73);
            this.btnDeleteImage.TabIndex = 2;
            this.btnDeleteImage.Text = "Delete";
            this.btnDeleteImage.UseVisualStyleBackColor = true;
            this.btnDeleteImage.Click += new System.EventHandler(this.btnDeleteImage_Click);
            // 
            // btnAddImage
            // 
            this.btnAddImage.Location = new System.Drawing.Point(87, 23);
            this.btnAddImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAddImage.Name = "btnAddImage";
            this.btnAddImage.Size = new System.Drawing.Size(81, 73);
            this.btnAddImage.TabIndex = 1;
            this.btnAddImage.Text = "Take Picture";
            this.btnAddImage.UseVisualStyleBackColor = true;
            this.btnAddImage.Click += new System.EventHandler(this.btnAddImage_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.button1);
            this.groupBox4.Controls.Add(this.btnDuplicateRoom);
            this.groupBox4.Controls.Add(this.btnEditRoom);
            this.groupBox4.Controls.Add(this.btnDelRoom);
            this.groupBox4.Controls.Add(this.btnAddRoom);
            this.groupBox4.Controls.Add(this.gridRooms);
            this.groupBox4.Location = new System.Drawing.Point(371, 11);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Size = new System.Drawing.Size(495, 478);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Room";
            // 
            // btnDuplicateRoom
            // 
            this.btnDuplicateRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDuplicateRoom.Location = new System.Drawing.Point(272, 23);
            this.btnDuplicateRoom.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDuplicateRoom.Name = "btnDuplicateRoom";
            this.btnDuplicateRoom.Size = new System.Drawing.Size(68, 73);
            this.btnDuplicateRoom.TabIndex = 4;
            this.btnDuplicateRoom.Text = "Clone";
            this.btnDuplicateRoom.UseVisualStyleBackColor = true;
            this.btnDuplicateRoom.Click += new System.EventHandler(this.btnDuplicateRoom_Click);
            // 
            // btnEditRoom
            // 
            this.btnEditRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditRoom.Location = new System.Drawing.Point(416, 23);
            this.btnEditRoom.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnEditRoom.Name = "btnEditRoom";
            this.btnEditRoom.Size = new System.Drawing.Size(68, 73);
            this.btnEditRoom.TabIndex = 3;
            this.btnEditRoom.Text = "Edit";
            this.btnEditRoom.UseVisualStyleBackColor = true;
            this.btnEditRoom.Click += new System.EventHandler(this.btnEditRoom_Click);
            // 
            // btnDelRoom
            // 
            this.btnDelRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelRoom.Location = new System.Drawing.Point(344, 23);
            this.btnDelRoom.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDelRoom.Name = "btnDelRoom";
            this.btnDelRoom.Size = new System.Drawing.Size(68, 73);
            this.btnDelRoom.TabIndex = 2;
            this.btnDelRoom.Text = "Delete";
            this.btnDelRoom.UseVisualStyleBackColor = true;
            this.btnDelRoom.Click += new System.EventHandler(this.btnDelRoom_Click);
            // 
            // btnAddRoom
            // 
            this.btnAddRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddRoom.Location = new System.Drawing.Point(128, 23);
            this.btnAddRoom.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAddRoom.Name = "btnAddRoom";
            this.btnAddRoom.Size = new System.Drawing.Size(68, 73);
            this.btnAddRoom.TabIndex = 1;
            this.btnAddRoom.Text = "Add";
            this.btnAddRoom.UseVisualStyleBackColor = true;
            this.btnAddRoom.Click += new System.EventHandler(this.btnAddRoom_Click);
            // 
            // gridRooms
            // 
            this.gridRooms.AllowUserToAddRows = false;
            this.gridRooms.AllowUserToDeleteRows = false;
            this.gridRooms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridRooms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridRooms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.room_number,
            this.PhotoFile,
            this.room_name});
            this.gridRooms.ContextMenuStrip = this.roomItemCtxMenu;
            this.gridRooms.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridRooms.Location = new System.Drawing.Point(12, 101);
            this.gridRooms.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gridRooms.MultiSelect = false;
            this.gridRooms.Name = "gridRooms";
            this.gridRooms.RowHeadersWidth = 51;
            this.gridRooms.RowTemplate.Height = 20;
            this.gridRooms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridRooms.Size = new System.Drawing.Size(472, 373);
            this.gridRooms.TabIndex = 0;
            this.gridRooms.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.btnEditRoom_Click);
            this.gridRooms.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridRooms_CellMouseDownClick);
            this.gridRooms.SelectionChanged += new System.EventHandler(this.roomsGridView_SelectionChanged);
            // 
            // room_number
            // 
            this.room_number.DataPropertyName = "room_number";
            this.room_number.HeaderText = "Number";
            this.room_number.MinimumWidth = 6;
            this.room_number.Name = "room_number";
            this.room_number.Width = 200;
            // 
            // PhotoFile
            // 
            this.PhotoFile.DataPropertyName = "PhotoFile";
            this.PhotoFile.FillWeight = 80F;
            this.PhotoFile.HeaderText = "PhotoFile";
            this.PhotoFile.MinimumWidth = 6;
            this.PhotoFile.Name = "PhotoFile";
            this.PhotoFile.ReadOnly = true;
            this.PhotoFile.Width = 80;
            // 
            // room_name
            // 
            this.room_name.DataPropertyName = "room_name";
            this.room_name.HeaderText = "Name";
            this.room_name.MinimumWidth = 6;
            this.room_name.Name = "room_name";
            this.room_name.Width = 600;
            // 
            // roomItemCtxMenu
            // 
            this.roomItemCtxMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.roomItemCtxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.duplicateRoomToolStripMenuItem});
            this.roomItemCtxMenu.Name = "roomItemCtxMenu";
            this.roomItemCtxMenu.Size = new System.Drawing.Size(106, 26);
            // 
            // duplicateRoomToolStripMenuItem
            // 
            this.duplicateRoomToolStripMenuItem.Name = "duplicateRoomToolStripMenuItem";
            this.duplicateRoomToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.duplicateRoomToolStripMenuItem.Text = "Clone";
            this.duplicateRoomToolStripMenuItem.Click += new System.EventHandler(this.duplicateRoomToolStripMenuItem_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.btnEditDepartment);
            this.groupBox2.Controls.Add(this.btnDelDepartment);
            this.groupBox2.Controls.Add(this.btnAddDepartment);
            this.groupBox2.Controls.Add(this.gridDepartments);
            this.groupBox2.Location = new System.Drawing.Point(8, 11);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(354, 478);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Department";
            // 
            // btnEditDepartment
            // 
            this.btnEditDepartment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditDepartment.Location = new System.Drawing.Point(272, 24);
            this.btnEditDepartment.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnEditDepartment.Name = "btnEditDepartment";
            this.btnEditDepartment.Size = new System.Drawing.Size(68, 73);
            this.btnEditDepartment.TabIndex = 3;
            this.btnEditDepartment.Text = "Edit";
            this.btnEditDepartment.UseVisualStyleBackColor = true;
            this.btnEditDepartment.Click += new System.EventHandler(this.btnEditDepartment_Click);
            // 
            // btnDelDepartment
            // 
            this.btnDelDepartment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelDepartment.Location = new System.Drawing.Point(200, 24);
            this.btnDelDepartment.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDelDepartment.Name = "btnDelDepartment";
            this.btnDelDepartment.Size = new System.Drawing.Size(68, 73);
            this.btnDelDepartment.TabIndex = 2;
            this.btnDelDepartment.Text = "Delete";
            this.btnDelDepartment.UseVisualStyleBackColor = true;
            this.btnDelDepartment.Click += new System.EventHandler(this.btnDelDepartment_Click);
            // 
            // btnAddDepartment
            // 
            this.btnAddDepartment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddDepartment.Location = new System.Drawing.Point(128, 24);
            this.btnAddDepartment.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAddDepartment.Name = "btnAddDepartment";
            this.btnAddDepartment.Size = new System.Drawing.Size(68, 73);
            this.btnAddDepartment.TabIndex = 1;
            this.btnAddDepartment.Text = "Add";
            this.btnAddDepartment.UseVisualStyleBackColor = true;
            this.btnAddDepartment.Click += new System.EventHandler(this.btnAddDepartment_Click);
            // 
            // gridDepartments
            // 
            this.gridDepartments.AllowUserToAddRows = false;
            this.gridDepartments.AllowUserToDeleteRows = false;
            this.gridDepartments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDepartments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDepartments.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Type,
            this.Description});
            this.gridDepartments.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridDepartments.Location = new System.Drawing.Point(8, 101);
            this.gridDepartments.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gridDepartments.MultiSelect = false;
            this.gridDepartments.Name = "gridDepartments";
            this.gridDepartments.RowHeadersWidth = 51;
            this.gridDepartments.RowTemplate.Height = 20;
            this.gridDepartments.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridDepartments.Size = new System.Drawing.Size(333, 373);
            this.gridDepartments.TabIndex = 0;
            this.gridDepartments.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.btnEditDepartment_Click);
            this.gridDepartments.SelectionChanged += new System.EventHandler(this.dptsGridView_SelectionChanged);
            // 
            // Type
            // 
            this.Type.DataPropertyName = "type";
            this.Type.HeaderText = "Type";
            this.Type.MinimumWidth = 6;
            this.Type.Name = "Type";
            this.Type.Visible = false;
            this.Type.Width = 300;
            // 
            // Description
            // 
            this.Description.DataPropertyName = "description";
            this.Description.HeaderText = "Description";
            this.Description.MinimumWidth = 6;
            this.Description.Name = "Description";
            this.Description.Width = 800;
            // 
            // ContentsTab
            // 
            this.ContentsTab.Controls.Add(this.assetInventoryGrpBox);
            this.ContentsTab.Controls.Add(this.groupBox1);
            this.ContentsTab.Location = new System.Drawing.Point(4, 29);
            this.ContentsTab.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ContentsTab.Name = "ContentsTab";
            this.ContentsTab.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ContentsTab.Size = new System.Drawing.Size(2293, 703);
            this.ContentsTab.TabIndex = 1;
            this.ContentsTab.Text = "Assets";
            this.ContentsTab.UseVisualStyleBackColor = true;
            // 
            // assetInventoryGrpBox
            // 
            this.assetInventoryGrpBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assetInventoryGrpBox.Controls.Add(this.btnDuplicateAsset);
            this.assetInventoryGrpBox.Controls.Add(this.cboInventoryPhotoState);
            this.assetInventoryGrpBox.Controls.Add(this.label2);
            this.assetInventoryGrpBox.Controls.Add(this.label1);
            this.assetInventoryGrpBox.Controls.Add(this.btnClearFilter);
            this.assetInventoryGrpBox.Controls.Add(this.gridInventory);
            this.assetInventoryGrpBox.Controls.Add(this.btnSaveAsset);
            this.assetInventoryGrpBox.Controls.Add(this.txtSearch);
            this.assetInventoryGrpBox.Controls.Add(this.btnDeleteAsset);
            this.assetInventoryGrpBox.Controls.Add(this.cboDepartmentFilter);
            this.assetInventoryGrpBox.Controls.Add(this.btnAddAsset);
            this.assetInventoryGrpBox.Controls.Add(this.label7);
            this.assetInventoryGrpBox.Controls.Add(this.cboRoomFilter);
            this.assetInventoryGrpBox.Controls.Add(this.label6);
            this.assetInventoryGrpBox.Location = new System.Drawing.Point(11, 14);
            this.assetInventoryGrpBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.assetInventoryGrpBox.Name = "assetInventoryGrpBox";
            this.assetInventoryGrpBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.assetInventoryGrpBox.Size = new System.Drawing.Size(915, 475);
            this.assetInventoryGrpBox.TabIndex = 40;
            this.assetInventoryGrpBox.TabStop = false;
            // 
            // btnDuplicateAsset
            // 
            this.btnDuplicateAsset.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.btnDuplicateAsset.Location = new System.Drawing.Point(708, 24);
            this.btnDuplicateAsset.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDuplicateAsset.Name = "btnDuplicateAsset";
            this.btnDuplicateAsset.Size = new System.Drawing.Size(64, 73);
            this.btnDuplicateAsset.TabIndex = 43;
            this.btnDuplicateAsset.Text = "Cl&one";
            this.btnDuplicateAsset.UseVisualStyleBackColor = true;
            this.btnDuplicateAsset.Click += new System.EventHandler(this.btnDuplicateAsset_Click);
            // 
            // cboInventoryPhotoState
            // 
            this.cboInventoryPhotoState.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboInventoryPhotoState.FormattingEnabled = true;
            this.cboInventoryPhotoState.Items.AddRange(new object[] {
            "-- Select State --",
            "-- No Photo --",
            "-- No Tag --",
            "-- No Photo/Tag --"});
            this.cboInventoryPhotoState.Location = new System.Drawing.Point(465, 51);
            this.cboInventoryPhotoState.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cboInventoryPhotoState.Name = "cboInventoryPhotoState";
            this.cboInventoryPhotoState.Size = new System.Drawing.Size(110, 28);
            this.cboInventoryPhotoState.TabIndex = 31;
            this.cboInventoryPhotoState.Text = "- Photo State -";
            this.cboInventoryPhotoState.SelectedIndexChanged += new System.EventHandler(this.cboInventoryPhotoState_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(463, 31);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 20);
            this.label2.TabIndex = 42;
            this.label2.Text = "Photo State";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 20);
            this.label1.TabIndex = 28;
            this.label1.Text = "Search";
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.btnClearFilter.Location = new System.Drawing.Point(578, 24);
            this.btnClearFilter.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(64, 73);
            this.btnClearFilter.TabIndex = 32;
            this.btnClearFilter.Text = "&Clear Filter";
            this.btnClearFilter.UseVisualStyleBackColor = true;
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // 
            // gridInventory
            // 
            this.gridInventory.AllowUserToAddRows = false;
            this.gridInventory.AllowUserToDeleteRows = false;
            this.gridInventory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridInventory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridInventory.ContextMenuStrip = this.inventoryItemCtxmenu;
            this.gridInventory.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridInventory.Location = new System.Drawing.Point(4, 101);
            this.gridInventory.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridInventory.MultiSelect = false;
            this.gridInventory.Name = "gridInventory";
            this.gridInventory.ReadOnly = true;
            this.gridInventory.RowHeadersWidth = 51;
            this.gridInventory.RowTemplate.Height = 24;
            this.gridInventory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridInventory.Size = new System.Drawing.Size(899, 354);
            this.gridInventory.TabIndex = 35;
            this.gridInventory.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.btnSaveAsset_Click);
            this.gridInventory.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridInventory_CellMouseDownClick);
            this.gridInventory.SelectionChanged += new System.EventHandler(this.gridInventory_SelectionChanged);
            // 
            // inventoryItemCtxmenu
            // 
            this.inventoryItemCtxmenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.inventoryItemCtxmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.duplicateInventoryToolStripMenuItem});
            this.inventoryItemCtxmenu.Name = "inventoryItemCtxmenu";
            this.inventoryItemCtxmenu.Size = new System.Drawing.Size(106, 26);
            // 
            // duplicateInventoryToolStripMenuItem
            // 
            this.duplicateInventoryToolStripMenuItem.Name = "duplicateInventoryToolStripMenuItem";
            this.duplicateInventoryToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.duplicateInventoryToolStripMenuItem.Text = "Clone";
            this.duplicateInventoryToolStripMenuItem.Click += new System.EventHandler(this.duplicateInventoryToolStripMenuItem_Click);
            // 
            // btnSaveAsset
            // 
            this.btnSaveAsset.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.btnSaveAsset.Location = new System.Drawing.Point(774, 24);
            this.btnSaveAsset.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSaveAsset.Name = "btnSaveAsset";
            this.btnSaveAsset.Size = new System.Drawing.Size(64, 73);
            this.btnSaveAsset.TabIndex = 34;
            this.btnSaveAsset.Text = "&Edit";
            this.btnSaveAsset.UseVisualStyleBackColor = true;
            this.btnSaveAsset.Click += new System.EventHandler(this.btnSaveAsset_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(7, 50);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(172, 26);
            this.txtSearch.TabIndex = 28;
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SearchInventoryGrid);
            // 
            // btnDeleteAsset
            // 
            this.btnDeleteAsset.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.btnDeleteAsset.Location = new System.Drawing.Point(840, 24);
            this.btnDeleteAsset.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDeleteAsset.Name = "btnDeleteAsset";
            this.btnDeleteAsset.Size = new System.Drawing.Size(65, 73);
            this.btnDeleteAsset.TabIndex = 35;
            this.btnDeleteAsset.Text = "&Delete";
            this.btnDeleteAsset.UseVisualStyleBackColor = true;
            this.btnDeleteAsset.Click += new System.EventHandler(this.btnDeleteAsset_Click);
            // 
            // cboDepartmentFilter
            // 
            this.cboDepartmentFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboDepartmentFilter.FormattingEnabled = true;
            this.cboDepartmentFilter.Location = new System.Drawing.Point(181, 50);
            this.cboDepartmentFilter.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cboDepartmentFilter.Name = "cboDepartmentFilter";
            this.cboDepartmentFilter.Size = new System.Drawing.Size(150, 28);
            this.cboDepartmentFilter.TabIndex = 29;
            this.cboDepartmentFilter.Text = "- Select a Department -";
            this.cboDepartmentFilter.SelectedIndexChanged += new System.EventHandler(this.cbDepartment_SelectedIndexChanged);
            // 
            // btnAddAsset
            // 
            this.btnAddAsset.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.btnAddAsset.Location = new System.Drawing.Point(644, 24);
            this.btnAddAsset.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAddAsset.Name = "btnAddAsset";
            this.btnAddAsset.Size = new System.Drawing.Size(64, 73);
            this.btnAddAsset.TabIndex = 33;
            this.btnAddAsset.Text = "&Add";
            this.btnAddAsset.UseVisualStyleBackColor = true;
            this.btnAddAsset.Click += new System.EventHandler(this.btnAddAsset_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(178, 30);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(156, 20);
            this.label7.TabIndex = 33;
            this.label7.Text = "Select a Department";
            // 
            // cboRoomFilter
            // 
            this.cboRoomFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRoomFilter.FormattingEnabled = true;
            this.cboRoomFilter.Location = new System.Drawing.Point(335, 50);
            this.cboRoomFilter.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cboRoomFilter.Name = "cboRoomFilter";
            this.cboRoomFilter.Size = new System.Drawing.Size(125, 28);
            this.cboRoomFilter.TabIndex = 30;
            this.cboRoomFilter.Text = "- Select a room -";
            this.cboRoomFilter.SelectedIndexChanged += new System.EventHandler(this.cbRoom_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(333, 30);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(114, 20);
            this.label6.TabIndex = 32;
            this.label6.Text = "Select a Room";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnSetAsTagInventory);
            this.groupBox1.Controls.Add(this.btnSetAsPhotoInventory);
            this.groupBox1.Controls.Add(this.btnBrowseInventory);
            this.groupBox1.Controls.Add(this.panelInventoryImages);
            this.groupBox1.Controls.Add(this.btnDelPictureInventory);
            this.groupBox1.Controls.Add(this.btnAddPictureInventory);
            this.groupBox1.Location = new System.Drawing.Point(941, 14);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(438, 475);
            this.groupBox1.TabIndex = 39;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Asset Images";
            // 
            // btnSetAsTagInventory
            // 
            this.btnSetAsTagInventory.Location = new System.Drawing.Point(341, 24);
            this.btnSetAsTagInventory.Margin = new System.Windows.Forms.Padding(1);
            this.btnSetAsTagInventory.Name = "btnSetAsTagInventory";
            this.btnSetAsTagInventory.Size = new System.Drawing.Size(79, 73);
            this.btnSetAsTagInventory.TabIndex = 10;
            this.btnSetAsTagInventory.Text = "Set as Tag";
            this.btnSetAsTagInventory.UseVisualStyleBackColor = true;
            this.btnSetAsTagInventory.Click += new System.EventHandler(this.btnSetAsTag_Click);
            // 
            // btnSetAsPhotoInventory
            // 
            this.btnSetAsPhotoInventory.Location = new System.Drawing.Point(260, 24);
            this.btnSetAsPhotoInventory.Margin = new System.Windows.Forms.Padding(1);
            this.btnSetAsPhotoInventory.Name = "btnSetAsPhotoInventory";
            this.btnSetAsPhotoInventory.Size = new System.Drawing.Size(79, 73);
            this.btnSetAsPhotoInventory.TabIndex = 9;
            this.btnSetAsPhotoInventory.Text = "Set as Photo";
            this.btnSetAsPhotoInventory.UseVisualStyleBackColor = true;
            this.btnSetAsPhotoInventory.Click += new System.EventHandler(this.btnSetAsAssetPhoto_Click);
            // 
            // btnBrowseInventory
            // 
            this.btnBrowseInventory.Location = new System.Drawing.Point(17, 24);
            this.btnBrowseInventory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnBrowseInventory.Name = "btnBrowseInventory";
            this.btnBrowseInventory.Size = new System.Drawing.Size(79, 73);
            this.btnBrowseInventory.TabIndex = 8;
            this.btnBrowseInventory.Text = "&Browse";
            this.btnBrowseInventory.UseVisualStyleBackColor = true;
            this.btnBrowseInventory.Click += new System.EventHandler(this.btnBrowseInventory_Click);
            // 
            // panelInventoryImages
            // 
            this.panelInventoryImages.AutoScroll = true;
            this.panelInventoryImages.Location = new System.Drawing.Point(17, 101);
            this.panelInventoryImages.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelInventoryImages.Name = "panelInventoryImages";
            this.panelInventoryImages.Size = new System.Drawing.Size(409, 361);
            this.panelInventoryImages.TabIndex = 7;
            // 
            // btnDelPictureInventory
            // 
            this.btnDelPictureInventory.Location = new System.Drawing.Point(179, 24);
            this.btnDelPictureInventory.Margin = new System.Windows.Forms.Padding(1);
            this.btnDelPictureInventory.Name = "btnDelPictureInventory";
            this.btnDelPictureInventory.Size = new System.Drawing.Size(79, 73);
            this.btnDelPictureInventory.TabIndex = 6;
            this.btnDelPictureInventory.Text = "Delete";
            this.btnDelPictureInventory.UseVisualStyleBackColor = true;
            this.btnDelPictureInventory.Click += new System.EventHandler(this.btnDelPictureInventory_Click);
            // 
            // btnAddPictureInventory
            // 
            this.btnAddPictureInventory.Location = new System.Drawing.Point(98, 24);
            this.btnAddPictureInventory.Margin = new System.Windows.Forms.Padding(1);
            this.btnAddPictureInventory.Name = "btnAddPictureInventory";
            this.btnAddPictureInventory.Size = new System.Drawing.Size(79, 73);
            this.btnAddPictureInventory.TabIndex = 5;
            this.btnAddPictureInventory.Text = "&Take Picture";
            this.btnAddPictureInventory.UseVisualStyleBackColor = true;
            this.btnAddPictureInventory.Click += new System.EventHandler(this.btnAddPictureInventory_Click);
            // 
            // audaxware_offlineDataSet
            // 
            this.audaxware_offlineDataSet.DataSetName = "audaxware_offlineDataSet";
            this.audaxware_offlineDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // audaxwareofflineDataSetBindingSource
            // 
            this.audaxwareofflineDataSetBindingSource.DataSource = this.audaxware_offlineDataSet;
            this.audaxwareofflineDataSetBindingSource.Position = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(200, 23);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(68, 73);
            this.button1.TabIndex = 4;
            this.button1.Text = "Move";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnMoveRoom_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1401, 560);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.project_name);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AudaxWare Offline Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.InventoryList_Load);
            this.DoubleClick += new System.EventHandler(this.LoadEditInventory);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.DeptRoomsTab.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridRooms)).EndInit();
            this.roomItemCtxMenu.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridDepartments)).EndInit();
            this.ContentsTab.ResumeLayout(false);
            this.assetInventoryGrpBox.ResumeLayout(false);
            this.assetInventoryGrpBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridInventory)).EndInit();
            this.inventoryItemCtxmenu.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.audaxware_offlineDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.audaxwareofflineDataSetBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label project_name;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.BindingSource audaxwareofflineDataSetBindingSource;
        private audaxware_offlineDataSet audaxware_offlineDataSet;
        private System.Windows.Forms.ToolStripMenuItem importFromAudaxwareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadToAudaxwareToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage DeptRoomsTab;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnEditRoom;
        private System.Windows.Forms.Button btnDelRoom;
        private System.Windows.Forms.Button btnAddRoom;
        private System.Windows.Forms.DataGridView gridRooms;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnEditDepartment;
        private System.Windows.Forms.Button btnDelDepartment;
        private System.Windows.Forms.Button btnAddDepartment;
        private System.Windows.Forms.DataGridView gridDepartments;
        private System.Windows.Forms.TabPage ContentsTab;
        private System.Windows.Forms.DataGridView gridInventory;
        private System.Windows.Forms.ComboBox cboRoomFilter;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cboDepartmentFilter;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnDeleteImage;
        private System.Windows.Forms.Button btnAddImage;
        private System.Windows.Forms.Button btnSaveAsset;
        private System.Windows.Forms.Button btnDeleteAsset;
        private System.Windows.Forms.Button btnAddAsset;
        private System.Windows.Forms.Panel panelImages;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelInventoryImages;
        private System.Windows.Forms.Button btnDelPictureInventory;
        private System.Windows.Forms.Button btnAddPictureInventory;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.Button btnBrowsePicture;
        private System.Windows.Forms.Button btnBrowseInventory;
        private System.Windows.Forms.ToolStripMenuItem loadManufacturesJSNCatalogFromAudaxwareToolStripMenuItem;
        private System.Windows.Forms.Button btnClearFilter;
        private System.Windows.Forms.ContextMenuStrip inventoryItemCtxmenu;
        private System.Windows.Forms.ToolStripMenuItem duplicateInventoryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip roomItemCtxMenu;
        private System.Windows.Forms.ToolStripMenuItem duplicateRoomToolStripMenuItem;
        private GroupBox assetInventoryGrpBox;
        private Button btnSetAsTagInventory;
        private Button btnSetAsPhotoInventory;
        private ComboBox cboInventoryPhotoState;
        private Label label2;
        private Button btnSetAsPhoto;
        private DataGridViewTextBoxColumn room_number;
        private DataGridViewTextBoxColumn PhotoFile;
        private DataGridViewTextBoxColumn room_name;
        private Button btnDuplicateAsset;
        private Button btnDuplicateRoom;
        private Button button1;
    }
}