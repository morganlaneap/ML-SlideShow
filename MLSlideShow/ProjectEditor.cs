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
using System.Drawing.Imaging;

namespace MLSlideShow
{
    public partial class ProjectEditor : Form
    {
        public const string APP_VERSION = "1.0";
        public Project currentProject { get; set; }
        public string currentProjectFilePath { get; set; }
        private IOHelper ioHelper = new IOHelper();
        private ImageHelper imageHelper = new ImageHelper();
        private bool isSavePending = false;
        private string oldTitle = "";

        public ProjectEditor()
        {
            InitializeComponent();
            CreateNewProject(false);
            UpdateStatus("Ready");
        }

        private void Exit(FormClosingEventArgs e)
        {
            if (isSavePending)
            {
                if (MessageBox.Show("You have unsaved changes. Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Application.Exit();
                } else
                {
                    e.Cancel = true;   
                }
            } else
            {
                Application.Exit();
            }
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
        private void SetTitle(string title, bool prefix = true)
        {
            var p = (prefix ? "MLSlideShow - " : "");

            this.Text = p + title;
        }

        public void MarkSavePending()
        {
            oldTitle = this.Text;
            isSavePending = true;
            SetTitle("*" + oldTitle, false);
        }

        private void ClearSavePending(bool prefix = false)
        {
            isSavePending = false;

            if (oldTitle == "")
            {
                oldTitle = currentProjectFilePath;
            }

            SetTitle(oldTitle, prefix);
        }

        private void CreateNewProject(bool prompt)
        {
            if (prompt)
            {
                if (MessageBox.Show("Are you sure you want to create a new project?", "New Project", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            Project project = new Project();
            project.ProjectName = "New Project";
            project.ProjectVersion = APP_VERSION;
            project.Images = new List<Models.Image>();
            project.DateCreated = DateTime.Now;
            currentProject = project;
            lstImages.Items.Clear();
            picturePreview.Image = null;
            this.Text = "MLSlideShow - Unsaved Project";
            isSavePending = false;
            currentProjectFilePath = "";
            lstImages.LargeImageList = null;
        }

        private void OpenProject()
        {
            if (isSavePending)
            {
                if (MessageBox.Show("You have unsaved changes. Are you sure you want to open a new project?", "Open Project", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            if (ofdMain.ShowDialog() == DialogResult.OK)
            {
                Project project = ioHelper.LoadProject(ofdMain.FileName);
                currentProject = project;
                BindList();
                currentProjectFilePath = ofdMain.FileName;
                SetTitle(currentProjectFilePath, true);
                oldTitle = currentProjectFilePath;
                ClearSavePending(true);
            }
        }

        public void SaveProject()
        {
            if (currentProjectFilePath != null && currentProjectFilePath != "")
            {
                ioHelper.SaveProject(currentProject, currentProjectFilePath);
                SetTitle(currentProjectFilePath);
                oldTitle = currentProjectFilePath;
                ClearSavePending(true);
            }
        }

        private void SaveProjectAs()
        {
            if (sfdMain.ShowDialog() == DialogResult.OK)
            {
                ioHelper.SaveProject(currentProject, sfdMain.FileName);
                currentProjectFilePath = sfdMain.FileName;
                SetTitle(currentProjectFilePath, true);
                oldTitle = currentProjectFilePath;
                ClearSavePending(true);
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
            if (ofdAddImage.ShowDialog() == DialogResult.OK)
            {
                Thread t = new Thread(() =>
                {
                    UpdateStatus("Indexing file...");

                    List<FileInfo> files = ioHelper.GetSingleImage(ofdAddImage.FileName);

                    LoadImages(files);
                });
                t.IsBackground = true;
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
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
        
        private void RemoveImageFromProject(string filePath)
        {
            currentProject.Images.Remove(currentProject.Images.Where(x => x.FilePath == filePath).First());

            Thread t = new Thread(() =>
            {
                BindList();
            });
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void ShowSettings()
        {
            SettingsForm sf = new SettingsForm(this);
            sf.Show();
        }
        #endregion

        #region Click Events
        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject(true);
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
            Exit(new FormClosingEventArgs(CloseReason.ApplicationExitCall, false));
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlaySlideShow();
        }

        private void tsbNewProject_Click(object sender, EventArgs e)
        {
            CreateNewProject(true);
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
            AddImage();
        }

        private void tsbPlay_Click(object sender, EventArgs e)
        {
            PlaySlideShow();
        }

        private void tsbExit_Click(object sender, EventArgs e)
        {
            Exit(new FormClosingEventArgs(CloseReason.ApplicationExitCall, false));
        }

        private void tsbSlideShowSettings_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettings();
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

                MarkSavePending();

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
                SlideShow ss = new SlideShow(currentProject.Images, this, delay: currentProject.SlideShowDelay);
                this.Hide();
                ss.Show();
            }
        }

        #endregion

        private void startSlideShowHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstImages.SelectedIndices.Count > 0)
            {
                int index = lstImages.SelectedIndices[0];
                SlideShow ss = new SlideShow(currentProject.Images, this, index, delay: currentProject.SlideShowDelay);
                this.Hide();
                ss.Show();
            }
        }

        private void removeImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstImages.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to remove this image?", "Remove Image", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var image = lstImages.SelectedItems[0];
                    RemoveImageFromProject(image.ToolTipText);
                }
            }
        }

        private void ProjectEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Exit(e);
        }        
    }
}
