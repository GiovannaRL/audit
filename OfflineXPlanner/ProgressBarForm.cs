using System;
using System.Windows.Forms;

namespace OfflineXPlanner
{
    public partial class ProgressBarForm : Form
    {                                                                                                           
        private int progressBarMaximum;

        public ProgressBarForm(string text, int maximum)
        {
            this.TopLevel = true;
            InitializeComponent();
            this.CenterToParent();
            this.progressBarLabel.Text = text;
            this.progressBarMaximum = maximum;
        }

        private void ProgressBarForm_Load(object sender, EventArgs e)
        {
            this.progressBar1.Maximum = this.progressBarMaximum;
            this.progressBar1.Step = 1;
        }

        public void PerformStep() {
            this.progressBar1.PerformStep();
        }

        public void UpdateMaximum(int value)
        {
            this.progressBar1.Maximum = value;
        }

        public int GetMaximum()
        {
            return this.progressBar1.Maximum;
        }

        public void UpdateLabel(string label)
        {
            this.progressBarLabel.Text = label;
        }

        public void IncementMaximum(int value)
        {
            this.progressBar1.Maximum += value;
        }
    }
}