using System;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Basler.Vision
{
    public class VisionHelper
    {
        public static byte[] Resize(IntPtr img, int w, int h, int sw, int sh)
        {
            //var buffer = IntPtr.Zero;
            Image<Gray, byte> img1 = null;
            Image<Gray, byte> simg = null;
            try
            {
                //buffer = Marshal.AllocHGlobal(img.Length);
                //Marshal.Copy(img, 0, buffer, img.Length);
                img1 = new Image<Gray, byte>(w, h, w, img);
                simg = img1.Resize(sw, sh, Emgu.CV.CvEnum.Inter.Nearest);
                
                return simg.Bytes;
            }
            finally
            {
                img1?.Dispose();
                simg?.Dispose();
                //Marshal.FreeHGlobal(img);
                //Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
