using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLSlideShow
{
    public partial class SettingsForm : Form
    {
        private ProjectEditor _editorReference { get; set; }

        public SettingsForm(ProjectEditor editorReference)
        {
            InitializeComponent();
            _editorReference = editorReference;
            txtSlideShowDelay.Text = (_editorReference.currentProject.SlideShowDelay / 1000).ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtSlideShowDelay.Text, out var t))
            {
                _editorReference.currentProject.SlideShowDelay = int.Parse((decimal.Parse(txtSlideShowDelay.Text) * 1000).ToString());
                _editorReference.SaveProject();
                this.Close();
            } else
            {
                MessageBox.Show("Please enter a valid delay value such as 1.3, 3.0 or 2.", "Error", MessageBoxButtons.OK);
            }
        }
    }
}
