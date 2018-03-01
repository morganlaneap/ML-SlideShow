using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MLSlideShow.Models;
using MLSlideShow.Helpers;
using System.IO;

namespace MLSlideShow
{
    public partial class ProjectEditor : Form
    {
        public const string APP_VERSION = "1.0";
        public Project currentProject { get; set; }
        public string currentProjectFilePath { get; set; }
        private IOHelper ioHelper = new IOHelper();

        public ProjectEditor()
        {
            InitializeComponent();
            CreateNewProject();
        }

        private void CreateNewProject()
        {
            Project project = new Project();
            project.ProjectName = "New Project";
            project.ProjectVersion = APP_VERSION;
            project.Images = new List<Models.Image>();
            project.DateCreated = DateTime.Now;
            currentProject = project;
            lstImages.Items.Clear();
            picturePreview.Image = null;
        }

        private void LoadImages(List<FileInfo> files)
        {
            foreach (FileInfo fi in files)
            {
                Models.Image i = new Models.Image();
                i.DateAdded = DateTime.Now;
                i.FileName = fi.Name;
                i.FilePath = fi.FullName;
                currentProject.Images.Add(i);
            }

            BindList();
        }

        private void BindList()
        {
            lstImages.Items.Clear();

            //List<System.Drawing.Image> imageList = new List<System.Drawing.Image>();
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(128, 128);
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            int imageIndex = 0;

            foreach (Models.Image i in currentProject.Images)
            {
                imageList.Images.Add(System.Drawing.Image.FromFile(i.FilePath));

                lstImages.Items.Add(new ListViewItem()
                {
                    Text = i.FileName,
                    ToolTipText = i.FilePath,
                    ImageIndex = imageIndex
                });

                imageIndex++;
            }

            lstImages.LargeImageList = imageList;
            lstImages.View = View.LargeIcon;
        }

        #region Click Events
        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdMain.ShowDialog() == DialogResult.OK)
            {
                ioHelper.SaveProject(currentProject, sfdMain.FileName);
                currentProjectFilePath = sfdMain.FileName;
            }
        }

        private void btnAddImageFolder_Click(object sender, EventArgs e)
        {
            if (fbdMain.ShowDialog() == DialogResult.OK)
            {
                List<FileInfo> files = ioHelper.GetImagesFromDirectory(fbdMain.SelectedPath);

                LoadImages(files);
            }
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentProjectFilePath != null && currentProjectFilePath != "")
            {
                ioHelper.SaveProject(currentProject, currentProjectFilePath);
            }
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == DialogResult.OK)
            {
                Project project = ioHelper.LoadProject(ofdMain.FileName);
                currentProject = project;
                BindList();
            }
        }

        private void lstImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstImages.SelectedItems.Count > 0)
            {
                picturePreview.Image = System.Drawing.Image.FromFile(lstImages.SelectedItems[0].ToolTipText);
            }
        }
        #endregion

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentProject.Images.Count == 0)
            {
                MessageBox.Show("You must add some images before starting a slideshow.", "Error");
            }
            else
            {
                SlideShow ss = new SlideShow(currentProject.Images, this);
                this.Hide();
                ss.Show();
            }
        }
    }
}
