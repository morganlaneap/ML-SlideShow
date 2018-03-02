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
            Size s = ResizeKeepAspect(new Size(b.Width, b.Height), nWidth, nHeight);
            Bitmap result = new Bitmap(s.Width, s.Height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(b, 0, 0, s.Width, s.Height);
            return result;
        }

        private Size ResizeKeepAspect(Size src, int maxWidth, int maxHeight)
        {
            decimal rnd = Math.Min(maxWidth / (decimal)src.Width, maxHeight / (decimal)src.Height);
            return new Size((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }
    }
}
