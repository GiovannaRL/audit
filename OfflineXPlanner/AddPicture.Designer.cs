namespace OfflineXPlanner
{
    partial class AddPicture
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnCaptureTagRight = new System.Windows.Forms.Button();
            this.btnCaptureTagLeft = new System.Windows.Forms.Button();
            this.btnAddImagesRight = new System.Windows.Forms.Button();
            this.cboCameraSelect = new System.Windows.Forms.ComboBox();
            this.btnCaptureRight = new System.Windows.Forms.Button();
            this.btnAddImagesLeft = new System.Windows.Forms.Button();
            this.btnCaptureLeft = new System.Windows.Forms.Button();
            this.lblCamera = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelImages = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRemoveImagens = new System.Windows.Forms.Button();
            this.btnCancelRight = new System.Windows.Forms.Button();
            this.btnCancelLeft = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.btnAddImagesRight);
            this.groupBox1.Controls.Add(this.btnCaptureTagRight);
            this.groupBox1.Controls.Add(this.btnCaptureTagLeft);
            this.groupBox1.Controls.Add(this.cboCameraSelect);
            this.groupBox1.Controls.Add(this.btnCaptureRight);
            this.groupBox1.Controls.Add(this.btnAddImagesLeft);
            this.groupBox1.Controls.Add(this.btnCaptureLeft);
            this.groupBox1.Controls.Add(this.lblCamera);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.panelImages);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 9);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(1430, 570);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Camera";
            // 
            // btnCaptureTagRight
            // 
            this.btnCaptureTagRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCaptureTagRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCaptureTagRight.Location = new System.Drawing.Point(1282, 370);
            this.btnCaptureTagRight.Margin = new System.Windows.Forms.Padding(2);
            this.btnCaptureTagRight.Name = "btnCaptureTagRight";
            this.btnCaptureTagRight.Size = new System.Drawing.Size(120, 120);
            this.btnCaptureTagRight.TabIndex = 37;
            this.btnCaptureTagRight.Text = "Capture Tag";
            this.btnCaptureTagRight.UseVisualStyleBackColor = true;
            this.btnCaptureTagRight.Click += new System.EventHandler(this.btnCaptureTagImage_Click);
            // 
            // btnCaptureTagLeft
            // 
            this.btnCaptureTagLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCaptureTagLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCaptureTagLeft.Location = new System.Drawing.Point(34, 369);
            this.btnCaptureTagLeft.Margin = new System.Windows.Forms.Padding(2);
            this.btnCaptureTagLeft.Name = "btnCaptureTagLeft";
            this.btnCaptureTagLeft.Size = new System.Drawing.Size(120, 120);
            this.btnCaptureTagLeft.TabIndex = 36;
            this.btnCaptureTagLeft.Text = "Capture Tag";
            this.btnCaptureTagLeft.UseVisualStyleBackColor = true;
            this.btnCaptureTagLeft.Click += new System.EventHandler(this.btnCaptureTagImage_Click);
            // 
            // btnAddImagesRight
            // 
            this.btnAddImagesRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnAddImagesRight.Enabled = false;
            this.btnAddImagesRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddImagesRight.Location = new System.Drawing.Point(1282, 114);
            this.btnAddImagesRight.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddImagesRight.Name = "btnAddImagesRight";
            this.btnAddImagesRight.Size = new System.Drawing.Size(120, 120);
            this.btnAddImagesRight.TabIndex = 35;
            this.btnAddImagesRight.Text = "Add Image(s)";
            this.btnAddImagesRight.UseVisualStyleBackColor = true;
            this.btnAddImagesRight.Click += new System.EventHandler(this.btnAddImages_Click);
            // 
            // cboCameraSelect
            // 
            this.cboCameraSelect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cboCameraSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboCameraSelect.FormattingEnabled = true;
            this.cboCameraSelect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cboCameraSelect.Location = new System.Drawing.Point(247, 48);
            this.cboCameraSelect.Margin = new System.Windows.Forms.Padding(2);
            this.cboCameraSelect.Name = "cboCameraSelect";
            this.cboCameraSelect.Size = new System.Drawing.Size(241, 37);
            this.cboCameraSelect.TabIndex = 28;
            this.cboCameraSelect.SelectedIndexChanged += new System.EventHandler(this.cbCameraSelect_SelectedIndexChanged);
            // 
            // btnCaptureRight
            // 
            this.btnCaptureRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCaptureRight.AutoSize = true;
            this.btnCaptureRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCaptureRight.Location = new System.Drawing.Point(1282, 241);
            this.btnCaptureRight.Name = "btnCaptureRight";
            this.btnCaptureRight.Size = new System.Drawing.Size(120, 120);
            this.btnCaptureRight.TabIndex = 32;
            this.btnCaptureRight.Text = "Capture";
            this.btnCaptureRight.UseVisualStyleBackColor = true;
            this.btnCaptureRight.Click += new System.EventHandler(this.btnCaptureImage_Click);
            // 
            // btnAddImagesLeft
            // 
            this.btnAddImagesLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAddImagesLeft.Enabled = false;
            this.btnAddImagesLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddImagesLeft.Location = new System.Drawing.Point(34, 114);
            this.btnAddImagesLeft.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddImagesLeft.Name = "btnAddImagesLeft";
            this.btnAddImagesLeft.Size = new System.Drawing.Size(120, 120);
            this.btnAddImagesLeft.TabIndex = 29;
            this.btnAddImagesLeft.Text = "Add Image(s)";
            this.btnAddImagesLeft.UseVisualStyleBackColor = true;
            this.btnAddImagesLeft.Click += new System.EventHandler(this.btnAddImages_Click);
            // 
            // btnCaptureLeft
            // 
            this.btnCaptureLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCaptureLeft.AutoSize = true;
            this.btnCaptureLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCaptureLeft.Location = new System.Drawing.Point(34, 241);
            this.btnCaptureLeft.Name = "btnCaptureLeft";
            this.btnCaptureLeft.Size = new System.Drawing.Size(120, 120);
            this.btnCaptureLeft.TabIndex = 33;
            this.btnCaptureLeft.Text = "Capture";
            this.btnCaptureLeft.UseVisualStyleBackColor = true;
            this.btnCaptureLeft.Click += new System.EventHandler(this.btnCaptureImage_Click);
            // 
            // lblCamera
            // 
            this.lblCamera.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCamera.AutoSize = true;
            this.lblCamera.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCamera.Location = new System.Drawing.Point(130, 61);
            this.lblCamera.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCamera.Name = "lblCamera";
            this.lblCamera.Size = new System.Drawing.Size(112, 17);
            this.lblCamera.TabIndex = 28;
            this.lblCamera.Text = "Select a Camera";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Location = new System.Drawing.Point(175, 114);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(535, 375);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            // 
            // panelImages
            // 
            this.panelImages.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelImages.AutoScroll = true;
            this.panelImages.BackColor = System.Drawing.Color.White;
            this.panelImages.Location = new System.Drawing.Point(719, 114);
            this.panelImages.Name = "panelImages";
            this.panelImages.Size = new System.Drawing.Size(535, 375);
            this.panelImages.TabIndex = 34;
            // 
            // btnRemoveImagens
            // 
            this.btnRemoveImagens.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnRemoveImagens.Enabled = false;
            this.btnRemoveImagens.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveImagens.Location = new System.Drawing.Point(655, 616);
            this.btnRemoveImagens.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveImagens.Name = "btnRemoveImagens";
            this.btnRemoveImagens.Size = new System.Drawing.Size(135, 146);
            this.btnRemoveImagens.TabIndex = 13;
            this.btnRemoveImagens.Text = "Remove All Images";
            this.btnRemoveImagens.UseVisualStyleBackColor = true;
            this.btnRemoveImagens.Click += new System.EventHandler(this.btnRemoveImagens_Click);
            // 
            // btnCancelRight
            // 
            this.btnCancelRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelRight.Location = new System.Drawing.Point(1270, 616);
            this.btnCancelRight.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancelRight.Name = "btnCancelRight";
            this.btnCancelRight.Size = new System.Drawing.Size(135, 146);
            this.btnCancelRight.TabIndex = 30;
            this.btnCancelRight.Text = "Cancel";
            this.btnCancelRight.UseVisualStyleBackColor = true;
            this.btnCancelRight.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnCancelLeft
            // 
            this.btnCancelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancelLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelLeft.Location = new System.Drawing.Point(22, 616);
            this.btnCancelLeft.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancelLeft.Name = "btnCancelLeft";
            this.btnCancelLeft.Size = new System.Drawing.Size(135, 146);
            this.btnCancelLeft.TabIndex = 31;
            this.btnCancelLeft.Text = "Cancel";
            this.btnCancelLeft.UseVisualStyleBackColor = true;
            this.btnCancelLeft.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // AddPicture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1443, 785);
            this.Controls.Add(this.btnCancelLeft);
            this.Controls.Add(this.btnCancelRight);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnRemoveImagens);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AddPicture";
            this.Text = "Capture Image";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.AddPicture_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cboCameraSelect;
        private System.Windows.Forms.Label lblCamera;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnRemoveImagens;
        private System.Windows.Forms.Button btnAddImagesLeft;
        private System.Windows.Forms.Button btnCancelRight;
        private System.Windows.Forms.Button btnCaptureRight;
        private System.Windows.Forms.Button btnCaptureLeft;
        private System.Windows.Forms.FlowLayoutPanel panelImages;
        private System.Windows.Forms.Button btnAddImagesRight;
        private System.Windows.Forms.Button btnCancelLeft;
        private System.Windows.Forms.Button btnCaptureTagRight;
        private System.Windows.Forms.Button btnCaptureTagLeft;        
    }
}