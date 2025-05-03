using OfflineXPlanner.Domain.Enums;
using System;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class ImportOptions : Form
    {
        public ImportMode selectedImportMode { get; set; }

        public ImportOptions()
        {
            InitializeComponent();
            this.CenterToParent();
        }

        private void completeImpBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("you are about to erase all existing data - are you sure?", "Overriding data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.selectedImportMode = ImportMode.overrideData;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void newDataOnlyBtn_Click(object sender, EventArgs e)
        {
            this.selectedImportMode = ImportMode.onlyNewData;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
