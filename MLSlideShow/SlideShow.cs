using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MLSlideShow
{
    public partial class SlideShow : Form
    {
        private List<Models.Image> imageList = new List<Models.Image>();
        private int currentIndex = 0;
        private ProjectEditor _oldForm { get; set; }

        public SlideShow(List<Models.Image> images, ProjectEditor oldForm)
        {
            InitializeComponent();
            _oldForm = oldForm;
            imageList = images;
            Thread slideShowThread = new Thread(RunSlideShow);
            slideShowThread.IsBackground = true;
            slideShowThread.SetApartmentState(ApartmentState.STA);
            slideShowThread.Start();
        }

        private void RunSlideShow()
        {
            Models.Image i = imageList[currentIndex];

            ChangeImage(i.FilePath);

            if (currentIndex == imageList.Count - 1)
            {
                currentIndex = 0;
            } else
            {
                currentIndex++;
            }

            Thread.Sleep(3000);
            RunSlideShow();
        }

        private delegate void delChangeImage(string path);
        private void ChangeImage(string path)
        {
            if (pictureCurrent.InvokeRequired)
            {
                pictureCurrent.Invoke(new delChangeImage(ChangeImage), path);
            } else
            {
                pictureCurrent.Image = System.Drawing.Image.FromFile(path);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit the SlideShow?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _oldForm.Show();
                this.Close();
            }            
        }
    }
}
