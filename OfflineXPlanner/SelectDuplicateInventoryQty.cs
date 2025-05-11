using OfflineXPlanner.Database;
using System;
using System.Linq;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class SelectDuplicateQtyForm : Form
    {
        private int itemID;
        public int? _newAssetId = null;

        public SelectDuplicateQtyForm(int itemID)
        {
            InitializeComponent();
            this.CenterToParent();

            this.itemID = itemID;
        }

        private void duplicateBtn_Click(object sender, System.EventArgs e)
        {
            IInventoryDAO inventoryDAO = new InventoryDAO();
            this.DialogResult = DialogResult.OK;

            if (Convert.ToInt32(quantityNuD.Value) < 1)
            {
                MessageBox.Show("Quantity must not be less than 1", "Quantity", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
            }

            var duplicatedItems = inventoryDAO.DuplicateItem(this.itemID, Convert.ToInt32(quantityNuD.Value));

            if (duplicatedItems.Count > 0)
            {
                _newAssetId = duplicatedItems.Last().Id;
            }
            else
            {
                MessageBox.Show("Error to try duplicate inventory item", "Duplicate Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Abort;
            }        

            this.Close();
        }

        private void quantityNuD_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
