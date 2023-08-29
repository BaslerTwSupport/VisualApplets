using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Basler.Vision
{
    public class VisionHelper
    {
        public static byte[] Resize(IntPtr img, int w, int h, int sw, int sh)
        {
            Image<Gray, byte> img1 = null;
            Image<Gray, byte> simg = null;
            try
            {
                img1 = new Image<Gray, byte>(w, h, w, img);
                simg = img1.Resize(sw, sh, Emgu.CV.CvEnum.Inter.Nearest);
                
                return simg.Bytes;
            }
            finally
            {
                img1?.Dispose();
                simg?.Dispose();
            }
        }
    }
}
