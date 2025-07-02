using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class SelectDuplicateQtyForm : Form
    {
        private int itemID;
        public int? _newAssetId = null;
        private int _projectId;
        private int _departmentId;
        private int _roomId;

        public SelectDuplicateQtyForm(int projectId, int departmentId, int roomId, int itemID)
        {
            InitializeComponent();
            this.CenterToParent();

            this.itemID = itemID;
            this._projectId = projectId;
            this._departmentId = departmentId;
            this._roomId = roomId;

            LoadDepartments();

        }

        private void duplicateBtn_Click(object sender, EventArgs e)
        {
            if (cboRoom.SelectedItem == null || cboDepartment.SelectedIndex < 1)
            {
                MessageBox.Show("Please select a department", "Duplicate Inventory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboRoom.SelectedItem == null || cboRoom.SelectedIndex < 1)
            {
                MessageBox.Show("Please select a room", "Duplicate Inventory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Convert.ToInt32(quantityNuD.Value) < 1)
            {
                MessageBox.Show("Quantity must not be less than 1", "Quantity", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
            }

            int selectedDepartmentId = (int)cboDepartment.SelectedValue;
            int selectedRoomId = (int)cboRoom.SelectedValue;

            IInventoryDAO inventoryDAO = new InventoryDAO();
            var duplicatedItems = inventoryDAO.DuplicateItem(itemID, Convert.ToInt32(quantityNuD.Value), selectedDepartmentId, selectedRoomId);

            if (duplicatedItems.Count > 0)
            {
                _newAssetId = duplicatedItems.Last().Id;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Error trying to duplicate inventory item", "Duplicate Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
            }

            this.Close();
        }

        private void quantityNuD_ValueChanged(object sender, EventArgs e)
        {

        }

        private void LoadDepartments()
        {
            IProjectDAO projectDAO = new ProjectDAO();
            var dept = cboDepartment.SelectedIndex;
            cboDepartment.DisplayMember = "description";
            cboDepartment.ValueMember = "department_id";
            cboDepartment.DataSource = projectDAO.GetDepartments(_projectId);

            cboDepartment.SelectedValue = _departmentId;
            cboDepartment.SelectedIndex = -1;

            LoadRooms(_departmentId);


        }

        private void LoadRooms(int departmentId)
        {
            IProjectDAO projectDAO = new ProjectDAO();
            var roomIndex = cboRoom.SelectedIndex;
            cboRoom.DisplayMember = "Room";
            cboRoom.ValueMember = "room_id";
            cboRoom.DataSource = projectDAO.GetRooms(
                _projectId,
                departmentId > 0 ? departmentId : (int?)null
            );
        }

        private void cboDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDepartment.SelectedValue != null && int.TryParse(cboDepartment.SelectedValue.ToString(), out int departmentId))
            {
                LoadRooms(departmentId);
            }
        }
    }
}
