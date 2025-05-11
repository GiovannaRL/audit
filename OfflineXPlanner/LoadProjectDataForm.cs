using OfflineXPlanner.Business;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using OfflineXPlanner.Utils;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class LoadProjectDataForm : Form
    {
        private bool isPreviousChosenLogin = false;

        public LoadProjectDataForm()
        {
            InitializeComponent();
            this.CenterToParent();
        }

        private void LoadProjectDataForm_Load(object sender, EventArgs e)
        {
            if (AudaxwareRestApiInfo.loggedDomain == null)
            {
                ProgressBarForm progressBarForm = new ProgressBarForm("Loading Enterprise Information", 2);
                progressBarForm.Show();

                Cursor.Current = Cursors.WaitCursor;
                var domains = DomainBusiness.GetDomains(false);
                progressBarForm.PerformStep();
                Cursor.Current = Cursors.Default;

                if (!ListUtil.isNull(domains) && domains.Count > 1)
                {
                    cbbDomains.DisplayMember = "name";
                    cbbDomains.DataSource = domains;

                    cbbDomains.SelectedItem = domains[0];

                    this.lblDomains.Show();
                    this.cbbDomains.Show();

                    int offSet = 50;
                    Point l = this.lblProjects.Location;
                    this.lblProjects.Location = new Point(l.X, l.Y + offSet);
                    l = this.projectsListBox.Location;
                    this.projectsListBox.Location = new Point(l.X, l.Y + offSet);
                    this.projectsListBox.Height -= offSet;
                    progressBarForm.PerformStep();

                }
                else if (!ListUtil.isEmptyOrNull(domains)) // must always have at least domain
                {
                    SelectDomain(domains[0]);
                    progressBarForm.PerformStep();
                }

                progressBarForm.Close();
            } else
            {
                isPreviousChosenLogin = true;
                SelectDomain(new DomainsRequestResponse()
                {
                    domain_id = (short)AudaxwareRestApiInfo.loggedDomain.domain_id,
                    created_at = AudaxwareRestApiInfo.loggedDomain.created_at,
                    name = AudaxwareRestApiInfo.loggedDomain.domain_name,
                    role_id = AudaxwareRestApiInfo.loggedDomain.role_id,
                    show_audax_info = AudaxwareRestApiInfo.loggedDomain.show_audax_info
                });
            }
        }

        private async Task LoadDomainProjects(DomainsRequestResponse domain)
        {
            ProgressBarForm progressBarForm = new ProgressBarForm("Loading Projects", 10);
            progressBarForm.Show();

            this.projectsListBox.Items.Clear();

            var projects = ProjectBusiness.Import();

            if (!ListUtil.isEmptyOrNull(projects))
            {
                this.projectsListBox.Items.AddRange(projects.ToArray());
                this.projectsListBox.ValueMember = "project_id";
                this.projectsListBox.DisplayMember = "description";
            } else
            {
                MessageBox.Show($"You have no projects available on {domain.name}", "Projects");
            }

            progressBarForm.Close();
        }

        private void SelectDomain(DomainsRequestResponse selectedDomain)
        {
            Cursor.Current = Cursors.WaitCursor;
            bool success = DomainBusiness.AddLoggedDomain(selectedDomain);
            Cursor.Current = Cursors.Default;

            LoadDomainProjects(selectedDomain);
        }

        private void cboDomains_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectDomain(this.cbbDomains.SelectedItem as DomainsRequestResponse);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (ImportOptions importOptions = new ImportOptions())
            {
                if (importOptions.ShowDialog() == DialogResult.OK)
                {
                    DomainBusiness.LoadChosenDomain(false);

                    Project selectedItem = projectsListBox.SelectedItem as Project;

                    ProjectBusiness.ImportData(selectedItem,
                        importOptions.selectedImportMode);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        public int SelectedProject
        {
            get { return Convert.ToInt32(((Project)projectsListBox.SelectedItem).project_id); }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!isPreviousChosenLogin)
            {
                AudaxwareRestApiInfo.loggedDomain = null;
            }
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
