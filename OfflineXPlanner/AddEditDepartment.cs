using Microsoft.Win32;
using OfflineXPlanner.Business;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class AddEditDepartment : Form
    {
        private int _projectId;
        private int _departmentId;
        public int? _newDepartmentId = null;
        
        public AddEditDepartment(int project_id)
        {
            LoadDepartment(project_id);
            btnAdd.Text = "Add";
        }

        public AddEditDepartment(int project_id, DataGridViewSelectedRowCollection department)
        {
            LoadDepartment(project_id);
            txtDescription.Text = department[0].Cells["description"].Value.ToString();
            _departmentId = (int)department[0].Cells["department_id"].Value;
            btnAdd.Text = "Save";
        }

        private void LoadDepartment(int project_id) {
            InitializeComponent();
            this.CenterToParent();
            this._projectId = project_id;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            if (!ValidFormUtil.AllNotNull(this.txtDescription.Text))
            {
                errorProvider1.SetError(txtDescription, "Description is required");
                return;
            }

            // Insert Department
            Department dpt = new Department();
            dpt.project_id = this._projectId;
            dpt.description = this.txtDescription.Text;
            dpt.type = "OTHER";

            if (_departmentId > 0)
            {
                dpt.department_id = _departmentId;
                DepartmentBusiness.UpdateDepartment(dpt);
            }
            else
            {
                DepartmentBusiness.InsertDepartment(dpt);
                _newDepartmentId = dpt.department_id;
            }
            
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtDescription_Changed(object sender, EventArgs e)
        {
            if (this.txtDescription != null && this.txtDescription.TextLength > 0)
            {
                this.btnAdd.Enabled = true;
                return;
            }

            this.btnAdd.Enabled = false;
        }
    }
}