namespace OfflineXPlanner
{
    partial class SelectDomain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDomain));
            this.cboDomains = new System.Windows.Forms.ComboBox();
            this.lblDomains = new System.Windows.Forms.Label();
            this.btnSelect = new System.Windows.Forms.Button();
            this.errorMessageLabel = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cboDomains
            // 
            this.cboDomains.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboDomains.FormattingEnabled = true;
            this.cboDomains.Location = new System.Drawing.Point(10, 50);
            this.cboDomains.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cboDomains.Name = "cboDomains";
            this.cboDomains.Size = new System.Drawing.Size(514, 28);
            this.cboDomains.TabIndex = 0;
            // 
            // lblDomains
            // 
            this.lblDomains.AutoSize = true;
            this.lblDomains.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDomains.Location = new System.Drawing.Point(8, 19);
            this.lblDomains.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDomains.Name = "lblDomains";
            this.lblDomains.Size = new System.Drawing.Size(516, 20);
            this.lblDomains.TabIndex = 1;
            this.lblDomains.Text = "Your user belongs to multiple enterprises, please select one to continue:";
            // 
            // btnSelect
            // 
            this.btnSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelect.Location = new System.Drawing.Point(304, 98);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(100, 35);
            this.btnSelect.TabIndex = 2;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // errorMessageLabel
            // 
            this.errorMessageLabel.AutoSize = true;
            this.errorMessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorMessageLabel.ForeColor = System.Drawing.Color.Red;
            this.errorMessageLabel.Location = new System.Drawing.Point(163, 175);
            this.errorMessageLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.errorMessageLabel.Name = "errorMessageLabel";
            this.errorMessageLabel.Size = new System.Drawing.Size(199, 16);
            this.errorMessageLabel.TabIndex = 3;
            this.errorMessageLabel.Text = "You need to select an enterprise";
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(424, 98);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SelectDomain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 160);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.errorMessageLabel);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.lblDomains);
            this.Controls.Add(this.cboDomains);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "SelectDomain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select your Enterprise";
            this.Load += new System.EventHandler(this.SelectDomain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboDomains;
        private System.Windows.Forms.Label lblDomains;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Label errorMessageLabel;
        private System.Windows.Forms.Button btnCancel;
    }
}