namespace OfflineXPlanner
{
    partial class SelectDuplicateQtyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDuplicateQtyForm));
            this.quantityNuD = new System.Windows.Forms.NumericUpDown();
            this.selectQtyLbl = new System.Windows.Forms.Label();
            this.duplicateBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.quantityNuD)).BeginInit();
            this.SuspendLayout();
            // 
            // quantityNuD
            // 
            this.quantityNuD.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.quantityNuD.Location = new System.Drawing.Point(123, 79);
            this.quantityNuD.Margin = new System.Windows.Forms.Padding(2);
            this.quantityNuD.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.quantityNuD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.quantityNuD.Name = "quantityNuD";
            this.quantityNuD.Size = new System.Drawing.Size(90, 26);
            this.quantityNuD.TabIndex = 1;
            this.quantityNuD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.quantityNuD.ValueChanged += new System.EventHandler(this.quantityNuD_ValueChanged);
            // 
            // selectQtyLbl
            // 
            this.selectQtyLbl.AutoSize = true;
            this.selectQtyLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectQtyLbl.Location = new System.Drawing.Point(94, 47);
            this.selectQtyLbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.selectQtyLbl.Name = "selectQtyLbl";
            this.selectQtyLbl.Size = new System.Drawing.Size(141, 20);
            this.selectQtyLbl.TabIndex = 2;
            this.selectQtyLbl.Text = "Select the quantity";
            // 
            // duplicateBtn
            // 
            this.duplicateBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.duplicateBtn.Location = new System.Drawing.Point(98, 132);
            this.duplicateBtn.Margin = new System.Windows.Forms.Padding(2);
            this.duplicateBtn.Name = "duplicateBtn";
            this.duplicateBtn.Size = new System.Drawing.Size(132, 35);
            this.duplicateBtn.TabIndex = 3;
            this.duplicateBtn.Text = "Duplicate";
            this.duplicateBtn.UseVisualStyleBackColor = true;
            this.duplicateBtn.Click += new System.EventHandler(this.duplicateBtn_Click);
            // 
            // SelectDuplicateQtyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 233);
            this.Controls.Add(this.duplicateBtn);
            this.Controls.Add(this.selectQtyLbl);
            this.Controls.Add(this.quantityNuD);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SelectDuplicateQtyForm";
            this.Text = "Quantity";
            ((System.ComponentModel.ISupportInitialize)(this.quantityNuD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown quantityNuD;
        private System.Windows.Forms.Label selectQtyLbl;
        private System.Windows.Forms.Button duplicateBtn;
    }
}