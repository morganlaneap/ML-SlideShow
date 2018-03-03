using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MLSlideShow.Models
{
    [XmlRoot("Project"), Serializable]
    public class Project
    {
        [XmlElement("ProjectName")]
        public string ProjectName { get; set; }

        [XmlElement("ProjectVersion")]
        public string ProjectVersion { get; set; }

        [XmlElement("DateCreated")]
        public DateTime DateCreated { get; set; }

        [XmlElement("SlideShowDelay")]
        public int SlideShowDelay { get; set; }

        [XmlArray("Images")]
        public List<Image> Images { get; set; }        
    }
}
