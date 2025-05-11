namespace OfflineXPlanner
{
    partial class ImportOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportOptions));
            this.lblImport = new System.Windows.Forms.Label();
            this.btnCompleteImp = new System.Windows.Forms.Button();
            this.btnNewDataOnly = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInformation = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblImport
            // 
            this.lblImport.AutoSize = true;
            this.lblImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImport.Location = new System.Drawing.Point(63, 30);
            this.lblImport.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImport.Name = "lblImport";
            this.lblImport.Size = new System.Drawing.Size(257, 20);
            this.lblImport.TabIndex = 0;
            this.lblImport.Text = "Please click on the loading method:";
            this.lblImport.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnCompleteImp
            // 
            this.btnCompleteImp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCompleteImp.Location = new System.Drawing.Point(58, 79);
            this.btnCompleteImp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCompleteImp.Name = "btnCompleteImp";
            this.btnCompleteImp.Size = new System.Drawing.Size(267, 35);
            this.btnCompleteImp.TabIndex = 1;
            this.btnCompleteImp.Text = "Overwrite*";
            this.btnCompleteImp.UseVisualStyleBackColor = true;
            this.btnCompleteImp.Click += new System.EventHandler(this.completeImpBtn_Click);
            // 
            // btnNewDataOnly
            // 
            this.btnNewDataOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewDataOnly.Location = new System.Drawing.Point(58, 128);
            this.btnNewDataOnly.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnNewDataOnly.Name = "btnNewDataOnly";
            this.btnNewDataOnly.Size = new System.Drawing.Size(267, 35);
            this.btnNewDataOnly.TabIndex = 2;
            this.btnNewDataOnly.Text = "Append";
            this.btnNewDataOnly.UseVisualStyleBackColor = true;
            this.btnNewDataOnly.Click += new System.EventHandler(this.newDataOnlyBtn_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(58, 177);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(267, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // lblInformation
            // 
            this.lblInformation.AutoSize = true;
            this.lblInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInformation.ForeColor = System.Drawing.Color.IndianRed;
            this.lblInformation.Location = new System.Drawing.Point(75, 229);
            this.lblInformation.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInformation.Name = "lblInformation";
            this.lblInformation.Size = new System.Drawing.Size(234, 16);
            this.lblInformation.TabIndex = 5;
            this.lblInformation.Text = "*Changes in existing assets will be lost";
            // 
            // ImportOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 271);
            this.Controls.Add(this.lblInformation);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnNewDataOnly);
            this.Controls.Add(this.btnCompleteImp);
            this.Controls.Add(this.lblImport);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "ImportOptions";
            this.Text = "Select Load Mode";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblImport;
        private System.Windows.Forms.Button btnCompleteImp;
        private System.Windows.Forms.Button btnNewDataOnly;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInformation;
    }
}