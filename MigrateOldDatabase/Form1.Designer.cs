namespace MigrateOldDatabase
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.domains = new System.Windows.Forms.ComboBox();
            this.projects = new System.Windows.Forms.CheckedListBox();
            this.database = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.users = new System.Windows.Forms.CheckBox();
            this.chkassets = new System.Windows.Forms.CheckBox();
            this.manufacturer = new System.Windows.Forms.CheckBox();
            this.categories = new System.Windows.Forms.CheckBox();
            this.equipment_code = new System.Windows.Forms.CheckBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.import_sunagmed = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(51, 492);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 34);
            this.button1.TabIndex = 0;
            this.button1.Text = "START";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // domains
            // 
            this.domains.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.domains.FormattingEnabled = true;
            this.domains.Location = new System.Drawing.Point(401, 81);
            this.domains.Margin = new System.Windows.Forms.Padding(4);
            this.domains.Name = "domains";
            this.domains.Size = new System.Drawing.Size(275, 33);
            this.domains.TabIndex = 1;
            this.domains.SelectedIndexChanged += new System.EventHandler(this.domains_SelectedIndexChanged);
            // 
            // projects
            // 
            this.projects.FormattingEnabled = true;
            this.projects.Location = new System.Drawing.Point(51, 220);
            this.projects.Name = "projects";
            this.projects.Size = new System.Drawing.Size(475, 242);
            this.projects.TabIndex = 3;
            // 
            // database
            // 
            this.database.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.database.FormattingEnabled = true;
            this.database.Location = new System.Drawing.Point(48, 81);
            this.database.Name = "database";
            this.database.Size = new System.Drawing.Size(275, 33);
            this.database.TabIndex = 4;
            this.database.SelectedIndexChanged += new System.EventHandler(this.database_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(103, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "MIGRATION LOCATION";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(490, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "ENTERPRISE";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(242, 174);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "PROJECTS";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(45, 473);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 17);
            this.label4.TabIndex = 8;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(51, 193);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(88, 21);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Select All";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(580, 174);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "label5";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(259, 492);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(108, 34);
            this.button3.TabIndex = 12;
            this.button3.Text = "FIX TAG";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Enabled = false;
            this.button4.Location = new System.Drawing.Point(401, 492);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(125, 34);
            this.button4.TabIndex = 14;
            this.button4.Text = "FIX COMMENT";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(259, 554);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(113, 48);
            this.button2.TabIndex = 15;
            this.button2.Text = "HSG Asset to Audaxware";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // users
            // 
            this.users.AutoSize = true;
            this.users.Location = new System.Drawing.Point(796, 170);
            this.users.Name = "users";
            this.users.Size = new System.Drawing.Size(67, 21);
            this.users.TabIndex = 16;
            this.users.Text = "Users";
            this.users.UseVisualStyleBackColor = true;
            // 
            // chkassets
            // 
            this.chkassets.AutoSize = true;
            this.chkassets.Location = new System.Drawing.Point(939, 197);
            this.chkassets.Name = "chkassets";
            this.chkassets.Size = new System.Drawing.Size(71, 21);
            this.chkassets.TabIndex = 21;
            this.chkassets.Text = "assets";
            this.chkassets.UseVisualStyleBackColor = true;
            // 
            // manufacturer
            // 
            this.manufacturer.AutoSize = true;
            this.manufacturer.Location = new System.Drawing.Point(796, 197);
            this.manufacturer.Name = "manufacturer";
            this.manufacturer.Size = new System.Drawing.Size(140, 21);
            this.manufacturer.TabIndex = 22;
            this.manufacturer.Text = "manufact./vendor";
            this.manufacturer.UseVisualStyleBackColor = true;
            // 
            // categories
            // 
            this.categories.AutoSize = true;
            this.categories.Location = new System.Drawing.Point(1083, 170);
            this.categories.Name = "categories";
            this.categories.Size = new System.Drawing.Size(96, 21);
            this.categories.TabIndex = 23;
            this.categories.Text = "categories";
            this.categories.UseVisualStyleBackColor = true;
            // 
            // equipment_code
            // 
            this.equipment_code.AutoSize = true;
            this.equipment_code.Location = new System.Drawing.Point(939, 170);
            this.equipment_code.Name = "equipment_code";
            this.equipment_code.Size = new System.Drawing.Size(131, 21);
            this.equipment_code.TabIndex = 27;
            this.equipment_code.Text = "equipment code";
            this.equipment_code.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(401, 554);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(125, 48);
            this.button5.TabIndex = 28;
            this.button5.Text = "Update assets $ (domain 1)";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(125, 554);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(108, 48);
            this.button6.TabIndex = 29;
            this.button6.Text = "FIX CUTSHEET";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(48, 637);
            this.button7.Margin = new System.Windows.Forms.Padding(4);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(211, 34);
            this.button7.TabIndex = 30;
            this.button7.Text = "Get file names (jpg, pdf)";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // import_sunagmed
            // 
            this.import_sunagmed.Location = new System.Drawing.Point(315, 637);
            this.import_sunagmed.Margin = new System.Windows.Forms.Padding(4);
            this.import_sunagmed.Name = "import_sunagmed";
            this.import_sunagmed.Size = new System.Drawing.Size(211, 34);
            this.import_sunagmed.TabIndex = 31;
            this.import_sunagmed.Text = "Import Sunagmed";
            this.import_sunagmed.UseVisualStyleBackColor = true;
            this.import_sunagmed.Click += new System.EventHandler(this.button8_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(48, 695);
            this.button8.Margin = new System.Windows.Forms.Padding(4);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(211, 34);
            this.button8.TabIndex = 32;
            this.button8.Text = "Check assets";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1266, 769);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.import_sunagmed);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.equipment_code);
            this.Controls.Add(this.categories);
            this.Controls.Add(this.manufacturer);
            this.Controls.Add(this.chkassets);
            this.Controls.Add(this.users);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.database);
            this.Controls.Add(this.projects);
            this.Controls.Add(this.domains);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox domains;
        private System.Windows.Forms.CheckedListBox projects;
        private System.Windows.Forms.ComboBox database;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox users;
        private System.Windows.Forms.CheckBox chkassets;
        private System.Windows.Forms.CheckBox manufacturer;
        private System.Windows.Forms.CheckBox categories;
        private System.Windows.Forms.CheckBox equipment_code;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button import_sunagmed;
        private System.Windows.Forms.Button button8;
    }
}

