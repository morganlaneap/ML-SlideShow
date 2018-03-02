using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        private ImageHelper imageHelper = new ImageHelper();

        public ProjectEditor()
        {
            InitializeComponent();
            CreateNewProject();
            UpdateStatus("Ready");
        }

        #region Status bar and thread safe code
        private delegate void delUpdateStatus(string status);
        private void UpdateStatus(string status)
        {
            if (mainToolstrip.InvokeRequired)
            {
                mainToolstrip.Invoke(new delUpdateStatus(UpdateStatus), status);
            } else
            {
                lblStatus.Text = status;
            }
        }
        #endregion

        #region Logic Methods
        private void SetTitle(string title)
        {
            this.Text = "MLSlideShow - " + title;
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
            this.Text = "MLSlideShow - Unsaved Project";
            currentProjectFilePath = "";
            lstImages.LargeImageList = null;
        }

        private void OpenProject()
        {
            if (ofdMain.ShowDialog() == DialogResult.OK)
            {
                Project project = ioHelper.LoadProject(ofdMain.FileName);
                currentProject = project;
                BindList();
                currentProjectFilePath = ofdMain.FileName;
                SetTitle(currentProjectFilePath);
            }
        }

        private void SaveProject()
        {
            if (currentProjectFilePath != null && currentProjectFilePath != "")
            {
                ioHelper.SaveProject(currentProject, currentProjectFilePath);
                SetTitle(currentProjectFilePath);
            }
        }

        private void SaveProjectAs()
        {
            if (sfdMain.ShowDialog() == DialogResult.OK)
            {
                ioHelper.SaveProject(currentProject, sfdMain.FileName);
                currentProjectFilePath = sfdMain.FileName;
                SetTitle(currentProjectFilePath);
            }
        }

        private void AddImageFolder()
        {
            if (fbdMain.ShowDialog() == DialogResult.OK)
            {
                Thread t = new Thread(() =>
                {
                    UpdateStatus("Indexing files...");

                    List<FileInfo> files = ioHelper.GetImagesFromDirectory(fbdMain.SelectedPath);

                    LoadImages(files);
                });
                t.IsBackground = true;
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }

        private void AddImage()
        {

        }

        private void LoadImages(List<FileInfo> files)
        {
            int index = 1;

            foreach (FileInfo fi in files)
            {
                UpdateStatus(String.Format("Locating image {0} of {1}", index, files.Count));

                Models.Image i = new Models.Image();
                i.DateAdded = DateTime.Now;
                i.FileName = fi.Name;
                i.FilePath = fi.FullName;
                currentProject.Images.Add(i);
            }

            UpdateStatus("Finalising...");

            BindList();
        }       
        #endregion

        #region Click Events
        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProjectAs();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySlideShow();
        }

        private void tsbNewProject_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }

        private void tsbOpenProject_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void tsbSaveProject_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void tsbSaveProjectAs_Click(object sender, EventArgs e)
        {
            SaveProjectAs();
        }

        private void tsbAddImageFolder_Click(object sender, EventArgs e)
        {
            AddImageFolder();
        }

        private void tsbAddImage_Click(object sender, EventArgs e)
        {

        }

        private void tsbPlay_Click(object sender, EventArgs e)
        {
            PlaySlideShow();
        }
        #endregion

        #region ListView events
        private delegate void delBindList();
        private void BindList()
        {
            if (lstImages.InvokeRequired)
            {
                lstImages.Invoke(new delBindList(BindList));
            }
            else
            {
                lstImages.Items.Clear();

                //List<System.Drawing.Image> imageList = new List<System.Drawing.Image>();
                ImageList imageList = new ImageList();
                imageList.ImageSize = new Size(128, 128);
                imageList.ColorDepth = ColorDepth.Depth32Bit;

                int imageIndex = 0;

                foreach (Models.Image i in currentProject.Images)
                {
                    UpdateStatus(String.Format("Loading image {0} of {1}", imageIndex + 1, currentProject.Images.Count));

                    Bitmap bmp = imageHelper.ResizeBitmap((Bitmap)System.Drawing.Image.FromFile(i.FilePath), 128, 128);

                    imageList.Images.Add(bmp);

                    lstImages.Items.Add(new ListViewItem()
                    {
                        Text = i.FileName,
                        ToolTipText = i.FilePath,
                        ImageIndex = imageIndex
                    });

                    imageIndex++;

                    Application.DoEvents();
                }

                lstImages.LargeImageList = imageList;
                lstImages.View = View.LargeIcon;

                lstImages.SelectedIndexChanged += lstImages_SelectedIndexChanged;

                UpdateStatus("Ready");
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

        #region SlideShow logic
        private void PlaySlideShow()
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
        #endregion        
    }
}
