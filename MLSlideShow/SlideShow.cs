﻿using System;
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
        private int _delay = 3000;
        private ProjectEditor _oldForm { get; set; }
        Thread slideShowThread;

        public SlideShow(List<Models.Image> images, ProjectEditor oldForm, int startFrom = 0, int delay = 3000)
        {
            InitializeComponent();
            currentIndex = startFrom;
            _oldForm = oldForm;
            _delay = delay;
            imageList = images;
            slideShowThread = new Thread(RunSlideShow);
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

            Thread.Sleep(_delay);
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
            //if (MessageBox.Show("Are you sure you want to exit the SlideShow?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
                slideShowThread.Abort();
                _oldForm.Show();
                this.Close();
            //}            
        }

        private void pictureCurrent_Click(object sender, EventArgs e)
        {
            ChangeImage(imageList[currentIndex + 1].FilePath);
        }
    }
}
