using System;
using System.Data;
using System.Windows.Forms;
using OfflineXPlanner.Domain;
using xPlannerCommon.Models;
using OfflineXPlanner.Business;
using OfflineXPlanner.Utils;
using System.Linq;

namespace OfflineXPlanner
{
    public partial class EditInventory : Form
    {
        int _projectId;
        int _departmentId;
        int _roomId;
        int _inventoryId = 0;
        int _id = 0;
        bool _isEdit = false;
        public int? _newAssetId = null;
        DataView _jsnData;
        static DataView _cachedJsnData;
        static DataView _cachedManufacturerData;

        string _lastSearch;
        bool _loading = true;

        public EditInventory(int projectId, int departmentId, int roomId)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            _loading = false;
            LoadData(projectId);
            groupBox6.Text = "Add Asset";
            btnSave.Text = "Add and Clo&se";
            btnAdd.Visible = true;
            cboDepartment.SelectedValue = departmentId;
            cboRoom.SelectedValue = roomId;
            cboJSN.SelectedIndex = -1;
            cboDescription.SelectedIndex = -1;
            cboManufacturer.SelectedValue = 673;
            _departmentId = departmentId;
            _roomId = roomId;
            this.CenterToParent();
            Cursor.Current = currentCursor;
            SetShortcuts();
        }
        public EditInventory(int projectId, DataGridViewRow inventory)
        {
            var currentCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            LoadData(projectId);
            groupBox6.Text = "Edit Asset";
            btnSave.Text = "&Save";
            btnAdd.Visible = false;
            _isEdit = true;

            cboDepartment.SelectedValue = inventory.Cells["department_id"].Value.ToString();
            cboRoom.SelectedValue = inventory.Cells["room_id"].Value.ToString();
            if (cboJSN.FindString(inventory.Cells["JSN"].Value.ToString()) > -1)
            {
                cboJSN.SelectedValue = inventory.Cells["JSN"].Value.ToString();
                cboDescription.SelectedValue = inventory.Cells["JSNNomenclature"].Value.ToString();
            }
            else
            {
                cboJSN.Text = inventory.Cells["JSN"].Value.ToString();
                cboDescription.Text = inventory.Cells["JSNNomenclature"].Value.ToString();
            }

            cboManufacturer.Text = inventory.Cells["Manufacturer"].Value.ToString();
            txtModel.Text = inventory.Cells["ModelName"].Value.ToString();
            cboU1.SelectedValue = inventory.Cells["U1"].Value.ToString();
            cboU2.SelectedValue = inventory.Cells["U2"].Value.ToString();
            cboU3.SelectedValue = inventory.Cells["U3"].Value.ToString();
            cboU4.SelectedValue = inventory.Cells["U4"].Value.ToString();
            cboU5.SelectedValue = inventory.Cells["U5"].Value.ToString();
            cboU6.SelectedValue = inventory.Cells["U6"].Value.ToString();
            txtECN.Text = inventory.Cells["ECN"].Value.ToString();
            cboPlacement.SelectedValue = inventory.Cells["InstallMethod"].Value;
            txtComments.Text = inventory.Cells["Comments"].Value.ToString();
            txtCadID.Text = inventory.Cells["CADID"].Value.ToString();
            txtHeight.Text = inventory.Cells["Height"].Value.ToString();
            txtWidth.Text = inventory.Cells["Width"].Value.ToString();
            txtMHeight.Text = inventory.Cells["MountingHeight"].Value.ToString();
            txtDepth.Text = inventory.Cells["Depth"].Value.ToString();
            txtCadID.Text = inventory.Cells["CADID"].Value.ToString();
            _inventoryId = (int)inventory.Cells["inventory_id"].Value;
            _id = (int)inventory.Cells["Id"].Value;
            _loading = false;
            Cursor.Current = currentCursor;
            SetShortcuts();
        }


        private void LoadData(int projectId)
        {
            InitializeComponent();

            this._projectId = projectId;

            cboDepartment.DisplayMember = "description";
            cboDepartment.ValueMember = "department_id";
            cboDepartment.DataSource = DepartmentBusiness.LoadDepartments(projectId);

            cboPlacement.DisplayMember = "Text";
            cboPlacement.ValueMember = "Value";

            var items = new[] {
                new { Text = "None", Value = "None" },
                new { Text = "Boom", Value = "Boom" },
                new { Text = "Ceiling", Value = "Ceiling" },
                new { Text = "Counter", Value = "Counter" },
                new { Text = "Floor", Value = "Floor" },
                new { Text = "Freestanding", Value = "Freestanding" },
                new { Text = "Mobile", Value = "Mobile" },
                new { Text = "On Cart", Value = "On Cart" },
                new { Text = "Other Equipment", Value = "Other Equipment" },
                new { Text = "Portable", Value = "Portable" },
                new { Text = "Recessed", Value = "Recessed" },
                new { Text = "Under-Counter", Value = "Under-Counter" },
                new { Text = "Wall", Value = "Wall" }
            };

            cboPlacement.DataSource = items;

            var u1 = new[] { new { Value = "", Text = "N/A" }, new { Value = "A", Text = "A: Hot and cold water" }, new { Value = "B", Text = "B: Cold water and drain" }, new { Value = "C", Text = "C: Hot water and drain" }, new { Value = "D", Text = "D: Cold and hot water and drain" }, new { Value = "E", Text = "E: Treated water and drain" }, new { Value = "F", Text = "F: Cold, hot and treated water and drain" }, new { Value = "G", Text = "G: Cold and treated water and drain" }, new { Value = "H", Text = "H: Hot and treated water and drain" }, new { Value = "I", Text = "I: Drain only" }, new { Value = "J", Text = "J: Cold water only" } };
            var u2 = new[] { new { Value = "", Text = "N/A" }, new { Value = "A", Text = "A: 120 volt, conventional outlet" }, new { Value = "B", Text = "B: 120 volt, special outlet" }, new { Value = "C", Text = "C: 208/220 volt" }, new { Value = "D", Text = "D: 120 and 208/220 volt" }, new { Value = "E", Text = "E: 440 volt, 3 phase" }, new { Value = "F", Text = "F: Special electrical requirements (includes, but is not limited to emergency power, multiple power connections, etc.)" }, new { Value = "G", Text = "G: 208/220 volt, 3 phase" } };
            var u3 = new[] { new { Value = "", Text = "N/A" }, new { Value = "A", Text = "A: Oxygen" }, new { Value = "B", Text = "B: Vacuum" }, new { Value = "C", Text = "C: Air, low pressure" }, new { Value = "D", Text = "D: Air, high pressure" }, new { Value = "E", Text = "E: Oxygen and medical air" }, new { Value = "H", Text = "H: Oxygen, vacuum and medical air" }, new { Value = "J", Text = "J: Vacuum and HP air" }, new { Value = "K", Text = "K: Medical air" } };
            var u4 = new[] { new { Value = "", Text = "N/A" }, new { Value = "A", Text = "A: Steam" }, new { Value = "B", Text = "B: Nitrogen gas" }, new { Value = "C", Text = "C: Nitrous oxide" }, new { Value = "D", Text = "D: Nitrogen and nitrous oxide gas" }, new { Value = "E", Text = "E: Carbon dioxide gas" }, new { Value = "F", Text = "F: Liquid carbon dioxide" }, new { Value = "G", Text = "G: Liquid nitrogen" }, new { Value = "H", Text = "H: Instrument Air" } };
            var u5 = new[] { new { Value = "", Text = "N/A" }, new { Value = "A", Text = "A: Natural gas" }, new { Value = "B", Text = "B: Liquid propane gas" }, new { Value = "C", Text = "C: Methane" }, new { Value = "D", Text = "D: Butane" }, new { Value = "E", Text = "E: Propane" }, new { Value = "F", Text = "F: Hydrogen gas" }, new { Value = "G", Text = "G: Reserved" }, new { Value = "H", Text = "H: Acetylene gas" } };
            var u6 = new[] { new { Value = "", Text = "N/A" }, new { Value = "A", Text = "A: Earth ground" }, new { Value = "B", Text = "B: Lead lined walls" }, new { Value = "C", Text = "C: Remote alarm ground" }, new { Value = "D", Text = "D: Empty conduit with pull cord" }, new { Value = "E", Text = "E: Vent to atmosphere" }, new { Value = "F", Text = "F: Special gas requirements" }, new { Value = "G", Text = "G: Liquid gas requirements" }, new { Value = "H", Text = "H: RF/Magnetic shielding" }, new { Value = "J", Text = "J: Wall/ceiling support required" }, new { Value = "K", Text = "K: Empty conduit/pull cord & wall/ceiling support required" }, new { Value = "M", Text = "M: Earth ground and wall/ceiling support required" }, new { Value = "P", Text = "P: Lead lined walls and wall/ceiling support required" }, new { Value = "T", Text = "T: CAT 6 wire to nearest Telecommunications Room" } };


            cboU1.DisplayMember = "Text";
            cboU1.ValueMember = "Value";
            cboU1.DataSource = u1;

            cboU2.DisplayMember = "Text";
            cboU2.ValueMember = "Value";
            cboU2.DataSource = u2;

            cboU3.DisplayMember = "Text";
            cboU3.ValueMember = "Value";
            cboU3.DataSource = u3;

            cboU4.DisplayMember = "Text";
            cboU4.ValueMember = "Value";
            cboU4.DataSource = u4;

            cboU5.DisplayMember = "Text";
            cboU5.ValueMember = "Value";
            cboU5.DataSource = u5;

            cboU6.DisplayMember = "Text";
            cboU6.ValueMember = "Value";
            cboU6.DataSource = u6;

            LoadJSN();
            LoadManufacturer();
            ClearFields();
            toolTip1.SetToolTip(lblU1, "Plumbing (Water and Drainage)");
            toolTip1.SetToolTip(lblU2, "Electrical");
            toolTip1.SetToolTip(lblU3, "Medical Gas (Provide operating pressures in accordance with NFPA 99)");
            toolTip1.SetToolTip(lblU4, "Miscellaneous Gas");
            toolTip1.SetToolTip(lblU5, "Non-Medical Gas");
            toolTip1.SetToolTip(lblU6, "Miscellaneous");
        }

        private void LoadManufacturer()
        {
            if (_cachedManufacturerData == null)
            {
                _cachedManufacturerData = new DataView(CatalogBusiness.LoadManufacturer());
            }
            cboManufacturer.DisplayMember = "description";
            cboManufacturer.ValueMember = "manufacturer_id";
            cboManufacturer.DataSource = _cachedManufacturerData;
        }

        private void LoadJSN()
        {
            if (_cachedJsnData == null)
            {
                _cachedJsnData = new DataView(CatalogBusiness.LoadJSNs());
            }
            _jsnData = _cachedJsnData;

            cboJSN.DisplayMember = "jsn_full";
            cboJSN.ValueMember = "jsnCode";
            cboJSN.DataSource = _jsnData;

            cboDescription.DisplayMember = "jsn_full_reverse";
            cboDescription.ValueMember = "jsnNomenclature";
            cboDescription.DataSource = _cachedJsnData;
            _lastSearch = null;
        }

        private void selectCam_Click(object sender, EventArgs e)
        {
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_isEdit)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            
        }

        private bool Save() {

            errorProvider1.Clear();
            var hasErrors = false;
            if (cboDepartment.Text == "")
            {
                errorProvider1.SetError(cboDepartment, "Department is required");
                hasErrors = true;
            }
            if (cboRoom.Text == "")
            {
                errorProvider1.SetError(cboRoom, "Room is required");
                hasErrors = true;
            }
            if (cboManufacturer.Text == "")
            {
                errorProvider1.SetError(cboManufacturer, "Manufacturer is required");
                hasErrors = true;
            }
            if (cboDescription.Text == "")
            {
                errorProvider1.SetError(cboDescription, "Description is required");
                hasErrors = true;
            }

            if (hasErrors)
                return false;

            var jsnCode = cboJSN.Text.Split('-')[0];
            var jsnNomenclature = cboDescription.Text;

            //add jsn if do not exist
            if ((cboJSN.SelectedValue == null && jsnCode != "") || (cboDescription.SelectedValue == null && jsnNomenclature != ""))
            {
                var jsn = new JSN();
                jsn.JSNCode = jsnCode;
                jsn.JSNNomenclature = jsnNomenclature;
                jsn.U1 = Convert.ToString(cboU1.SelectedValue);
                jsn.U2 = Convert.ToString(cboU2.SelectedValue);
                jsn.U3 = Convert.ToString(cboU3.SelectedValue);
                jsn.U4 = Convert.ToString(cboU4.SelectedValue);
                jsn.U5 = Convert.ToString(cboU5.SelectedValue);
                jsn.U6 = Convert.ToString(cboU6.SelectedValue);

                CatalogBusiness.InsertOrUpdateJSN(jsn);
            }
            else
            {
                if (cboJSN.SelectedValue != null)
                    jsnCode = cboJSN.SelectedValue.ToString();
                if (cboDescription.SelectedValue != null)
                    jsnNomenclature = cboDescription.SelectedValue.ToString();
            }

            asset_inventory inventory = new asset_inventory();
            inventory.jsn_code = jsnCode;
            inventory.jsn_nomenclature = jsnNomenclature;
            inventory.asset_description = jsnNomenclature;
            inventory.jsn_utility1 = Convert.ToString(cboU1.SelectedValue);
            inventory.jsn_utility2 = Convert.ToString(cboU2.SelectedValue);
            inventory.jsn_utility3 = Convert.ToString(cboU3.SelectedValue);
            inventory.jsn_utility4 = Convert.ToString(cboU4.SelectedValue);
            inventory.jsn_utility5 = Convert.ToString(cboU5.SelectedValue);
            inventory.jsn_utility6 = Convert.ToString(cboU6.SelectedValue);
            inventory.comment = txtComments.Text;
            inventory.cad_id = txtCadID.Text;
            inventory.manufacturer_description = cboManufacturer.Text;
            inventory.serial_name = txtModel.Text;
            inventory.department_id = (int)cboDepartment.SelectedValue;
            inventory.room_id = (int)cboRoom.SelectedValue;
            inventory.project_id = _projectId;
            inventory.height = txtHeight.Text;
            inventory.width = txtWidth.Text;
            inventory.depth = txtDepth.Text;
            inventory.department_description = cboDepartment.Text;
            var room = cboRoom.Text;
            int separator = room.IndexOf(" -- ");
            inventory.room_name = room.Substring(0, separator);
            inventory.room_number = room.Substring(separator + 4);
            inventory.inventory_id = _id;
            inventory.ECN = txtECN.Text;
            inventory.cad_id = txtCadID.Text;
            inventory.placement = cboPlacement.SelectedValue?.ToString();
            inventory.mounting_height = txtMHeight.Text;
            
            Inventory inv = new Inventory(inventory);
            inv.DateAdded = DateTime.Now; 
            if (_id == 0)
            {
                InventoryBusiness.InsertInventory(_projectId, (int)cboDepartment.SelectedValue, (int)cboRoom.SelectedValue, inv);
                _newAssetId = inv.Id;
            }              
                
            else
                InventoryBusiness.UpdateInventory(_projectId, (int)cboDepartment.SelectedValue, (int)cboRoom.SelectedValue, inv);

            return true;

        }

        private void cboDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDepartment.SelectedValue != null)
            {
                cboRoom.DisplayMember = "room";
                cboRoom.ValueMember = "room_id";
                cboRoom.DataSource = RoomBusiness.Get(_projectId, (int)cboDepartment.SelectedValue);
            }
        }

        private void cboJSN_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }
            FillJSNFields(true);
        }

        private void FillJSNFields(bool isJSNField)
        {

            if (isJSNField)
            {
                if (cboJSN.SelectedIndex == -1)
                {
                    cboDescription.SelectedIndexChanged -= cboDescription_SelectedIndexChanged;
                    cboDescription.SelectedIndex = -1;
                    cboDescription.SelectedIndexChanged += cboDescription_SelectedIndexChanged;
                    FillUtilitiesEmpty();
                }
                else
                {
                    var jsn = CatalogBusiness.LoadJSN(cboJSN.SelectedValue.ToString());
                    FillUtilities(jsn);
                    cboDescription.SelectedIndexChanged -= cboDescription_SelectedIndexChanged;
                    cboDescription.SelectedValue = jsn.nomenclature;
                    cboDescription.SelectedIndexChanged += cboDescription_SelectedIndexChanged;
                }
            }
            else
            {
                if (cboDescription.SelectedIndex > -1)
                {
                    if (cboDescription.Text != "")
                    {
                        var splited = cboDescription.Text.Split('-');
                        var jsnCode = splited[splited.Length-1].Trim();
                        var jsn = CatalogBusiness.LoadJSN(jsnCode);
                        FillUtilities(jsn);
                        cboJSN.SelectedIndexChanged -= cboJSN_SelectedIndexChanged;
                        cboJSN.SelectedValue = jsn.jsn_code;
                        cboJSN.SelectedIndexChanged += cboJSN_SelectedIndexChanged;
                    }
                }
                else
                {
                    FillUtilitiesEmpty();
                    cboJSN.SelectedIndexChanged -= cboJSN_SelectedIndexChanged;
                    cboJSN.SelectedIndex = -1;
                    cboJSN.SelectedIndexChanged += cboJSN_SelectedIndexChanged;
                }
            }
        }

        private void FillUtilitiesEmpty()
        {
            cboU1.SelectedIndex = -1;
            cboU2.SelectedIndex = -1;
            cboU3.SelectedIndex = -1;
            cboU4.SelectedIndex = -1;
            cboU5.SelectedIndex = -1;
            cboU6.SelectedIndex = -1;
        }

        private void FillUtilities(jsn jsn)
        {
            if (_loading)
            {
                return;
            }
            cboU1.SelectedValue = jsn?.utility1 ?? "";
            cboU2.SelectedValue = jsn?.utility2 ?? "";
            cboU3.SelectedValue = jsn?.utility3 ?? "";
            cboU4.SelectedValue = jsn?.utility4 ?? "";
            cboU5.SelectedValue = jsn?.utility5 ?? "";
            cboU6.SelectedValue = jsn?.utility6 ?? "";
        }

        private void cboDescription_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }
            FillJSNFields(false);
        }

        private string RemoveLastPart(string description) {
            var arr = description.Split('-');
            var returnText = "";
            var i = 0;
            foreach (var item in arr)
            {
                if (i+1 < arr.Length)
                {
                    returnText += item;
                }
                i++;
            }

            return returnText;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                MessageBox.Show("Asset added!", "Add Asset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            string jsnCode = cboJSN.Text.Split('-')[0].Trim();
            string manufacturer = cboManufacturer.Text.Trim();

            if (!CheckIfJsnExists(jsnCode))            
                _cachedJsnData = null;            

            if (!CheckIfManufacuterExists(manufacturer))            
                _cachedManufacturerData = null;
            

            // Clear data
            LoadJSN();
            LoadManufacturer();
            ClearFields();
            txtSearch.Focus();
        }

        private bool CheckIfJsnExists(string jsnCode)
        {
            if (_cachedJsnData != null)
            {
                bool exists = _cachedJsnData.Table.AsEnumerable().Any(row => row["jsnCode"].ToString() == jsnCode);
                return exists;
            }
            return false;
        }

        private bool CheckIfManufacuterExists(string manufacturer)
        {
            if (_cachedManufacturerData != null)
            {
                bool exists = _cachedManufacturerData.Table.AsEnumerable().Any(row => row["description"].ToString() == manufacturer);
                return exists;
            }
            return false;
        }


        private void updateSearch_Tick(object sender, EventArgs e)
        {
            updateSearch.Enabled = false;
            var selectedIndex = cboDescription.SelectedIndex;
            if (_cachedJsnData == null)
                return;
            if (_lastSearch == txtSearch.Text)
                return;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                _cachedJsnData.RowFilter = null;
            }
            else
            {
                _cachedJsnData.RowFilter = $"JSNNomenclature LIKE '%{txtSearch.Text}%' OR JSNCode LIKE '%{txtSearch.Text}%'";
            }
            _lastSearch = txtSearch.Text;
            cboDescription.Refresh();
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                _loading = true;
            }
            cboDescription.DataSource = _cachedJsnData;
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                cboDescription.SelectedIndex = selectedIndex;
                _loading = false;
            }
        }

        private void ClearFields()
        {
            cboJSN.SelectedIndex = -1;
            cboJSN.SelectedItem = null;
            cboPlacement.SelectedIndex = -1;
            txtHeight.Text = null;
            txtDepth.Text = null;
            txtECN.Text = null;
            txtComments.Text = null;
            txtCadID.Text = null;
            txtMHeight.Text = null;
            txtModel.Text = null;
            txtWidth.Text = null;
            txtSearch.Text = null;
            cboManufacturer.SelectedValue = 673; 
            cboU1.SelectedIndex = -1;
            cboU2.SelectedIndex = -1;
            cboU3.SelectedIndex = -1;
            cboU4.SelectedIndex = -1;
            cboU5.SelectedIndex = -1;
            cboU6.SelectedIndex = -1;
        }


        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            updateSearch.Enabled = true;
        }

        private void SetShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(EditInventory_KeyDown);
        }

        private void EditInventory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.R)
            {
                ClearFields();
            }
            if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
            }
        }
    }
}
