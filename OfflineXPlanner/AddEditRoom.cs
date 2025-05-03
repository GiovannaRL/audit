using System;
using System.Windows.Forms;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Business;
using OfflineXPlanner.Utils;

namespace OfflineXPlanner
{
    public partial class AddEditRoom : Form
    {

        int _projectId;
        int _departmentId;
        int _roomId;
        public int? _newRoomId = null;
        bool _isDuplicate;

        public AddEditRoom(int projectId, int departmentId)
        {
            LoadRoomParams(projectId, departmentId, false);
            btnAdd.Text = "Add";
        }

        public AddEditRoom(int projectId, int departmentId, int room_id, string roomNumber, string roomName)
        {
            LoadRoomParams(projectId, departmentId, true);
            btnAdd.Text = "Duplicate";
            _roomId = room_id;
            txtRoomName.Text = roomName;
            txtRoomNumber.Text = roomNumber;
        }

        public AddEditRoom(int projectId, int departmentId, DataGridViewSelectedRowCollection room)
        {
            LoadRoomParams(projectId, departmentId, false);

            txtRoomName.Text = room[0].Cells["room_name"].Value.ToString();
            txtRoomNumber.Text = room[0].Cells["room_number"].Value.ToString();
            btnAdd.Text = "Save";
            _roomId = (int)room[0].Cells["room_id"].Value;
        }

        private void LoadRoomParams(int projectId, int departmentId, bool isDuplicate) {
            _projectId = projectId;
            _departmentId = departmentId;
            _isDuplicate = isDuplicate;

            InitializeComponent();
            this.CenterToParent();
            cboDepartment.DisplayMember = "description";
            cboDepartment.ValueMember = "department_id";
            cboDepartment.DataSource = DepartmentBusiness.LoadDepartments(projectId);
            cboDepartment.SelectedValue = departmentId;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            if (!ValidFormUtil.AllNotNull(txtRoomName.Text))
            {
                errorProvider1.SetError(txtRoomName, "Room Name is required");
                return;
            }

            if (cboDepartment.SelectedIndex < 0)
            {
                errorProvider1.SetError(txtRoomName, "Please select a department");
                return;

            }


            var room = new Room
            {
                ProjectId = _projectId,
                DepartmentId = (int)cboDepartment.SelectedValue,
                Number = txtRoomNumber.Text,
                Name = txtRoomName.Text
            };

            var ok = true;

            if (_isDuplicate)
            {
                room.Id = 0;
                this.DialogResult = DialogResult.Abort;
                var insertedRoom = RoomBusiness.DuplicateRoom(_projectId, _departmentId, _roomId, room);
                if (insertedRoom != null)
                {
                    this.DialogResult = DialogResult.OK; 
                }
                _newRoomId = insertedRoom.Id;
                this.Close();
                return;
            }
            else
            {
                _departmentId = (int)cboDepartment.SelectedValue;
            }

            if (_roomId > 0)
            {
                room.Id = _roomId;
                ok = RoomBusiness.Update(room);
            }
            else
            {
                ok = RoomBusiness.Insert(room);
            }

            if (ok)
            {
                _newRoomId = room.Id;
                this.Close();
                return;
            }
            MessageBox.Show("Error to add room");
        }

        private void txtDescription_Changed(object sender, EventArgs e)
        {
            if ((this.txtRoomNumber != null && this.txtRoomName != null) && (this.txtRoomNumber.TextLength > 0 && this.txtRoomName.TextLength > 0))
            {
                this.btnAdd.Enabled = true;
                return;
            }

            this.btnAdd.Enabled = false;
        }

    }
}
