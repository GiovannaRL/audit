using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OfflineXPlanner.Database;
using OfflineXPlanner.Utils;
using System.Drawing;
using OfflineXPlanner.Business;
using OfflineXPlanner.Domain;
using System.IO;

namespace OfflineXPlanner
{
    public partial class ProjectList : Form
    {
        private bool isImportData = false;
        private bool isExportData = false;
        private List<Project> projects;
        Main mainForm;

        public bool loadProjectsBtnClick = false;

        public ProjectList(Main form, bool isImportData, bool isExportData)
        {
            this.isImportData = isImportData;
            this.isExportData = isExportData;
            this.mainForm = form;
            InitializeComponent();
            this.CenterToParent();
        }

        public ProjectList(Main form, bool isImportData, bool isExportData, List<Project> projects)
        {
            this.isImportData = isImportData;
            this.isExportData = isExportData;
            this.mainForm = form;
            this.projects = projects;
            InitializeComponent();
        }

        private void SetPageInformation()
        {
            if (cboProjects.Items.Count == 0)
            {
                this.openProjectBtn.Enabled = false;
                this.removeProjectBtn.Enabled = false;
                this.cboProjects.Enabled = false;
            }
            else
            {
                this.importProjectsBtn.Visible = isImportData && !isExportData;
                this.cboProjects.Enabled = true;
                this.openProjectBtn.Visible = !isImportData && !isExportData;
                this.removeProjectBtn.Visible = !isImportData && !isExportData;
            }

            this.btnExportData.Visible = !isImportData && isExportData;

            this.Text = isImportData ? "Load from Audaxware" : isExportData ? "Send to Audaxware" : "Open Project";

            this.importProjectsBtn.Text = isImportData ? "Load project data" : "Load from Audaxware...";
        }

        private void ConfigureThreeButtons()
        {
            this.importProjectsBtn.Location = new Point(56, 115);
            this.importProjectsBtn.Margin = new Padding(2);
            this.importProjectsBtn.Visible = true;
            this.btnCancel.Location = new Point(56, 152);
            this.Height = 250;
        }

        private void PopulateProjectsList(List<Project> projects)
        {
            var items = new List<ComboBoxItems>();
            foreach (var project in projects)
            {
                items.Add(new ComboBoxItems { Id = project.project_id, Name = project.description });
            }

            cboProjects.DisplayMember = "Name";
            cboProjects.ValueMember = "Id";
            cboProjects.DataSource = items;
        }

        private void ProjectList_Load(object sender, EventArgs e)
        {
            LoadProject();
        }

        private void LoadProject() {
            IProjectDAO projectDAO = new ProjectDAO();
            if (ListUtil.isEmptyOrNull(projects))
            {
                projects = projectDAO.GetProjects();
            }

            if (ListUtil.isEmptyOrNull(projects) && !isImportData && !isExportData)
            {
                ConfigureThreeButtons();
            }
            else
            {
                PopulateProjectsList(projects);
            }

            SetPageInformation();
        }

        public int SelectedProject
        {
            get { return Convert.ToInt32(cboProjects.SelectedValue); }
        }

        public string SelectedProjectName
        {
            get { return cboProjects.Text; }
        }

        private void openProjectBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void removeProjectBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This project will only be deleted at the offline tool but it will continue to exist in Audaxware. Are you sure you want to delete it?", "Delete Project", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var projectId = (int)cboProjects.SelectedValue;
                if (ProjectBusiness.DeleteProject(projectId) > 0)
                {
                    string RoomImagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audaxware", "project_" + projectId.ToString());
                    if (Directory.Exists(RoomImagesPath))
                        Directory.Delete(RoomImagesPath, true);

                    var projectPos = projects.FindIndex(x => x.project_id == projectId);
                    projects.Remove(projects[projectPos]);
                    cboProjects.DataSource = null;
                    cboProjects.Items.Clear();
                    LoadProject();
                    //reload main page if the project that we just deleted was open
                    if (mainForm._selectedProjectId == projectId)
                    {
                        mainForm.LoadDepartmentsGridData(0);
                        mainForm.LoadInventory(0);
                    }
                }
                
            }

        }

        private void exportDataBtn_Click(object sender, EventArgs e)
        {
            // Export data
            Cursor.Current = Cursors.WaitCursor;
            InventoryBusiness.Export(Convert.ToInt32(cboProjects.SelectedValue));
            Cursor.Current = Cursors.Default;
        }

        private void ChangeToImportProjectDataDesign()
        {
            this.isImportData = true;
            this.isExportData = !this.isImportData;
            this.Text = "Load project data";
        }

        private async void importProjectsBtn_Click(object sender, EventArgs e)
        {
            if (!isImportData)
            {
                this.loadProjectsBtnClick = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                using (ImportOptions importOptions = new ImportOptions())
                {

                    if (importOptions.ShowDialog() == DialogResult.OK)
                    {

                        ProjectBusiness.ImportData(
                           new Project(Convert.ToInt32(cboProjects.SelectedValue),
                           ((ComboBoxItems)cboProjects.SelectedItem).Name),
                            importOptions.selectedImportMode);

                        openProjectBtn_Click(null, null);
                    }
                }
            }
        }
    }
}
