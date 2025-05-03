using OfflineXPlanner.Business;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using System;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class SelectDomain : Form
    {
        public SelectDomain()
        {
            InitializeComponent();
            this.CenterToParent();
        }

        private void SelectDomain_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var domains = DomainBusiness.GetDomains(false);
            Cursor.Current = Cursors.Default;

            cboDomains.DisplayMember = "name";
            cboDomains.DataSource = domains;

            cboDomains.SelectedItem = domains[0];
        }

        private async void selectButton_Click(object sender, EventArgs e)
        {
            if (this.cboDomains.SelectedItem == null)
            {
                this.errorMessageLabel.Show();
                return;
            }

            this.errorMessageLabel.Hide();

            var selectedDomain = this.cboDomains.SelectedItem as DomainsRequestResponse;

            Cursor.Current = Cursors.WaitCursor;
            bool success = DomainBusiness.AddLoggedDomain(selectedDomain);
            Cursor.Current = Cursors.Default;
            AudaxwareRestApiInfo.loggedDomain = new DomainInfo(selectedDomain);
            DomainBusiness.StoreChosenDomain(AudaxwareRestApiInfo.loggedDomain);
            this.DialogResult = DialogResult.OK;
            this.Close();

            //TODO: throw exception when success is false
        }
    }
}
