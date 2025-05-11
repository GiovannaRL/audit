namespace OfflineXPlanner
{
    partial class ProjectList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectList));
            this.cboProjects = new System.Windows.Forms.ComboBox();
            this.lblSelectProject = new System.Windows.Forms.Label();
            this.openProjectBtn = new System.Windows.Forms.Button();
            this.removeProjectBtn = new System.Windows.Forms.Button();
            this.importProjectsBtn = new System.Windows.Forms.Button();
            this.btnExportData = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cboProjects
            // 
            this.cboProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboProjects.FormattingEnabled = true;
            this.cboProjects.Location = new System.Drawing.Point(40, 38);
            this.cboProjects.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cboProjects.Name = "cboProjects";
            this.cboProjects.Size = new System.Drawing.Size(292, 28);
            this.cboProjects.TabIndex = 0;
            // 
            // lblSelectProject
            // 
            this.lblSelectProject.AutoSize = true;
            this.lblSelectProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectProject.Location = new System.Drawing.Point(36, 16);
            this.lblSelectProject.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSelectProject.Name = "lblSelectProject";
            this.lblSelectProject.Size = new System.Drawing.Size(62, 20);
            this.lblSelectProject.TabIndex = 2;
            this.lblSelectProject.Text = "Project:";
            // 
            // openProjectBtn
            // 
            this.openProjectBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openProjectBtn.Location = new System.Drawing.Point(72, 84);
            this.openProjectBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.openProjectBtn.Name = "openProjectBtn";
            this.openProjectBtn.Size = new System.Drawing.Size(220, 35);
            this.openProjectBtn.TabIndex = 5;
            this.openProjectBtn.Text = "Open";
            this.openProjectBtn.UseVisualStyleBackColor = true;
            this.openProjectBtn.Click += new System.EventHandler(this.openProjectBtn_Click);
            // 
            // removeProjectBtn
            // 
            this.removeProjectBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.removeProjectBtn.Location = new System.Drawing.Point(72, 132);
            this.removeProjectBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.removeProjectBtn.Name = "removeProjectBtn";
            this.removeProjectBtn.Size = new System.Drawing.Size(220, 35);
            this.removeProjectBtn.TabIndex = 5;
            this.removeProjectBtn.Text = "Delete";
            this.removeProjectBtn.UseVisualStyleBackColor = true;
            this.removeProjectBtn.Click += new System.EventHandler(this.removeProjectBtn_Click);
            // 
            // importProjectsBtn
            // 
            this.importProjectsBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.importProjectsBtn.Location = new System.Drawing.Point(72, 84);
            this.importProjectsBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.importProjectsBtn.Name = "importProjectsBtn";
            this.importProjectsBtn.Size = new System.Drawing.Size(220, 35);
            this.importProjectsBtn.TabIndex = 6;
            this.importProjectsBtn.Text = "Load from Audaxware...";
            this.importProjectsBtn.UseVisualStyleBackColor = true;
            this.importProjectsBtn.Click += new System.EventHandler(this.importProjectsBtn_Click);
            // 
            // btnExportData
            // 
            this.btnExportData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportData.Location = new System.Drawing.Point(72, 84);
            this.btnExportData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnExportData.Name = "btnExportData";
            this.btnExportData.Size = new System.Drawing.Size(220, 35);
            this.btnExportData.TabIndex = 7;
            this.btnExportData.Text = "Send to Audaxware...";
            this.btnExportData.UseVisualStyleBackColor = true;
            this.btnExportData.Click += new System.EventHandler(this.exportDataBtn_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(72, 181);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(220, 35);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ProjectList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 269);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExportData);
            this.Controls.Add(this.importProjectsBtn);
            this.Controls.Add(this.openProjectBtn);
            this.Controls.Add(this.removeProjectBtn);
            this.Controls.Add(this.lblSelectProject);
            this.Controls.Add(this.cboProjects);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProjectList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Project";
            this.Load += new System.EventHandler(this.ProjectList_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboProjects;
        private System.Windows.Forms.Label lblSelectProject;
        private System.Windows.Forms.Button openProjectBtn;
        private System.Windows.Forms.Button removeProjectBtn;
        private System.Windows.Forms.Button importProjectsBtn;
        private System.Windows.Forms.Button btnExportData;
        private System.Windows.Forms.Button btnCancel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}