using OfflineXPlanner.Business;
using OfflineXPlanner.Security;
using OfflineXPlanner.Utils;
using System;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class Login : Form
    {
        private bool displayDomainsSelect = false;

        public Login(bool displayDomains = false)
        {
            InitializeComponent();
            this.CenterToParent();

            this.displayDomainsSelect = displayDomains;
        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            // Hide error message if exists
            this.errorMessageLabel.Hide();
            errorProvider1.Clear();

            // Verify fields
            if (!ValidFormUtil.AllNotNull(this.txtUsername.Text, this.txtPassword.Text))
            {
                errorProvider1.SetError(txtUsername, "Please specify username and password to proceed.");
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            bool loginDone = LoginBusiness.Login(this.txtUsername.Text, this.txtPassword.Text);
            Cursor.Current = Cursors.Default;
            if (loginDone)
            {
                DomainBusiness.LoadChosenDomain(this.displayDomainsSelect);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // Otherwise, displays error message
                MessageBox.Show("Error to try logging in", "Login");
            }
        }
    }
}
