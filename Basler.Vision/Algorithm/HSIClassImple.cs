using Basler.Base.Vision;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Basler.Base;
using Emgu.CV.Ocl;
using Emgu.CV.Util;

namespace Basler.Vision.Algorithm
{
    public class HSIClassImple : IHSIClass
    {
        private Mat _kernel5x5;
        private Mat _kernel11x11;
        public HSIClassImple()
        {
            _kernel5x5 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(5, 5), new Point(2, 2));
            _kernel11x11 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(11, 11), new Point(2, 2));
        }
        public byte[] Execute((IntPtr bytes, int width, int height) image, (ResultSelection result, PixelFormat pxielFormat, Point H, Point S, Point I) p)
        {
            Image<Gray, byte> imgBayer = null;
            var imgRGB = new Image<Bgr, byte>(image.width, image.height);
            var imgHsv = new Image<Bgr, byte>(image.width, image.height);
            Image<Gray, byte>[] images = null;
            Image<Gray, byte> r, s, i, opening, closing;
            r = null; s = null; i = null; opening = null; closing = null;
            //var buffer = IntPtr.Zero;
            try
            {
                var format = ColorConversion.BayerBg2Rgb;
                if(p.pxielFormat == PixelFormat.GB)
                {
                    format = ColorConversion.BayerGb2Rgb;
                }
                else if (p.pxielFormat == PixelFormat.GR)
                {
                    format = ColorConversion.BayerGr2Rgb;
                }
                else if (p.pxielFormat == PixelFormat.BG)
                {
                    format = ColorConversion.BayerBg2Rgb;
                }
                else
                {
                    format = ColorConversion.BayerRg2Rgb;
                }
                //buffer = Marshal.AllocHGlobal(image.bytes.Length);
                //Marshal.Copy(image.bytes, 0, buffer, image.bytes.Length);
                imgBayer = new Image<Gray, byte>(image.width, image.height, image.width, image.bytes);
                CvInvoke.CvtColor(imgBayer, imgRGB, format);
                if(p.result == ResultSelection.Red)
                {
                    return imgRGB.Split()[0].Bytes;
                }
                else if (p.result == ResultSelection.Green)
                {
                    return imgRGB.Split()[1].Bytes;
                }
                else if (p.result == ResultSelection.Blue)
                {
                    return imgRGB.Split()[2].Bytes;
                }
                CvInvoke.CvtColor(imgRGB, imgHsv, ColorConversion.Bgr2Hsv);
                if (p.result == ResultSelection.Hue)
                {
                    return imgHsv.Split()[0].Bytes;
                }
                else if (p.result == ResultSelection.Saturation)
                {
                    return imgHsv.Split()[1].Bytes;
                }
                else if (p.result == ResultSelection.Intensity)
                {
                    return imgHsv.Split()[2].Bytes;
                }
                images = imgHsv.Split();
                var hT = images[0].ThresholdBinary(new Gray(p.H.X), new Gray(p.H.Y));
                var sT = images[1].ThresholdBinary(new Gray(p.S.X), new Gray(p.S.Y));
                var vT = images[2].ThresholdBinary(new Gray(p.I.X), new Gray(p.I.Y));
                var t = (hT & sT) & (hT & vT);
                //return t.Bytes;
                opening = new Image<Gray, byte>(t.Size);
                closing = new Image<Gray, byte>(t.Size);
                CvInvoke.MorphologyEx(t, opening, MorphOp.Open, _kernel5x5, new Point(1, 1), 1, BorderType.Default, new MCvScalar(0));
                CvInvoke.MorphologyEx(opening, closing, MorphOp.Close, _kernel11x11, new Point(1, 1), 1, BorderType.Default, new MCvScalar(0));
                r = closing.ThresholdBinary(new Gray(1), new Gray(255));
                //s = images[1] & closing;
                //i = images[2] & closing;
                //imgHsv.Dispose();
                //imgHsv = new Image<Bgr, byte>(image.width, image.height);
                //CvInvoke.Merge(new VectorOfMat(r.Mat, r.Mat, r.Mat), imgHsv.Mat);
                return r.Bytes;
            }
            finally
            {
                imgBayer?.Dispose();
                imgRGB?.Dispose();
                imgHsv?.Dispose();
                if (images != null)
                {
                    for (int j = 0; j < images.Length; j++)
                    {
                        images[j]?.Dispose();
                    }
                }
                r?.Dispose();
                s?.Dispose();
                i?.Dispose();
                opening?.Dispose();
                closing?.Dispose();
                //Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
