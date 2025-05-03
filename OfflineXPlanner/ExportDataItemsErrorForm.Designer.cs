namespace OfflineXPlanner
{
    partial class ExportDataItemsErrorForm
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
            this.itemsErrorGridView = new System.Windows.Forms.DataGridView();
            this.itemsErrorLabel = new System.Windows.Forms.Label();
            this.okBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.itemsErrorGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // itemsErrorGridView
            // 
            this.itemsErrorGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.itemsErrorGridView.Location = new System.Drawing.Point(9, 43);
            this.itemsErrorGridView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.itemsErrorGridView.Name = "itemsErrorGridView";
            this.itemsErrorGridView.RowTemplate.Height = 24;
            this.itemsErrorGridView.Size = new System.Drawing.Size(832, 466);
            this.itemsErrorGridView.TabIndex = 0;
            // 
            // itemsErrorLabel
            // 
            this.itemsErrorLabel.AutoSize = true;
            this.itemsErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.itemsErrorLabel.Location = new System.Drawing.Point(79, 17);
            this.itemsErrorLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.itemsErrorLabel.Name = "itemsErrorLabel";
            this.itemsErrorLabel.Size = new System.Drawing.Size(680, 20);
            this.itemsErrorLabel.TabIndex = 1;
            this.itemsErrorLabel.Text = "The following errors were found on the items to be uploaded. Please fix them and " +
    "then try again.";
            // 
            // okBtn
            // 
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okBtn.Location = new System.Drawing.Point(351, 532);
            this.okBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(156, 35);
            this.okBtn.TabIndex = 2;
            this.okBtn.Text = "Ok";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okButton_Click);
            // 
            // ExportDataItemsErrorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 595);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.itemsErrorLabel);
            this.Controls.Add(this.itemsErrorGridView);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "ExportDataItemsErrorForm";
            this.Text = "Errors to be fixed";
            this.Load += new System.EventHandler(this.ExportDataItemsErrorForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.itemsErrorGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView itemsErrorGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.DataGridViewTextBoxColumn JSN;
        private System.Windows.Forms.DataGridViewTextBoxColumn Resp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comment;
        private System.Windows.Forms.DataGridViewTextBoxColumn Manufacturer;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModelNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModelName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Phase;
        private System.Windows.Forms.DataGridViewTextBoxColumn Department;
        private System.Windows.Forms.DataGridViewTextBoxColumn Room;
        private System.Windows.Forms.DataGridViewTextBoxColumn U1;
        private System.Windows.Forms.DataGridViewTextBoxColumn U2;
        private System.Windows.Forms.DataGridViewTextBoxColumn U3;
        private System.Windows.Forms.DataGridViewTextBoxColumn U4;
        private System.Windows.Forms.DataGridViewTextBoxColumn U5;
        private System.Windows.Forms.DataGridViewTextBoxColumn U6;
        private System.Windows.Forms.Label itemsErrorLabel;
        private System.Windows.Forms.Button okBtn;
    }
}