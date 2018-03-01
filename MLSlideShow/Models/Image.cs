using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MLSlideShow.Models
{
    [XmlRoot("Image"), Serializable]
    public class Image
    {
        [XmlElement("FilePath")]
        public string FilePath { get; set; }

        [XmlElement("DateAdded")]
        public DateTime DateAdded { get; set; }

        [XmlElement("FileName")]
        public string FileName { get; set; }
    }
}
