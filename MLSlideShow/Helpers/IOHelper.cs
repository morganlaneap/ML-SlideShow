using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MLSlideShow.Models;
using System.Xml.Serialization;

namespace MLSlideShow.Helpers
{
    public class IOHelper
    {
        public bool SaveProject(Project project, string fileName)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            TextWriter textWriter = new StreamWriter(fileName);
            xml.Serialize(textWriter, project);
            textWriter.Close();            
            return true;
        }

        public List<FileInfo> GetImagesFromDirectory(string directory)
        {
            List<FileInfo> files = new List<FileInfo>();

            foreach (FileInfo fi in new DirectoryInfo(directory).EnumerateFiles())
            {
                if (Variables.AllowedFileTypes.Contains(fi.Extension))
                {
                    files.Add(fi);
                }
            }

            return files;
        }

        public Project LoadProject(string path)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            StreamReader reader = new StreamReader(path);
            return (Project)xml.Deserialize(reader.BaseStream);
        }
    }
}
