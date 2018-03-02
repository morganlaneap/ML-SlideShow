using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLSlideShow.Helpers
{
    public class ImageHelper
    {
        public Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(b, 0, 0, nWidth, nHeight);
            return result;
        }
    }
}
