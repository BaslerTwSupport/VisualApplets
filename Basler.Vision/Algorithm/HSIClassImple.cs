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
            Image<Gray, byte>  r, s, i, opening, closing;           
            r = null; s = null; i = null; opening = null; closing = null;
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
                CvInvoke.CvtColor(imgRGB, imgHsv, ColorConversion.Rgb2HsvFull);
                images = imgHsv.Split();
                if (p.result == ResultSelection.Hue)
                {
                    return images[2].Bytes;
                }
                else if (p.result == ResultSelection.Saturation)
                {
                    return images[1].Bytes;
                }
                else if (p.result == ResultSelection.Intensity)
                {
                    return images[0].Bytes;
                }
                var gMax = new Gray(255);
                var hTMin = images[2].ThresholdBinary(new Gray(p.H.X), gMax);
                var hTMax = images[2].ThresholdBinaryInv(new Gray(p.H.Y), gMax);
                var hT = hTMin & hTMax;
                var sTMin = images[1].ThresholdBinary(new Gray(p.S.X), gMax);
                var sTMax = images[1].ThresholdBinaryInv(new Gray(p.S.Y), gMax);
                var sT = sTMin & sTMax;
                var vTMin = images[0].ThresholdBinary(new Gray(p.I.X), gMax);
                var vTMax = images[0].ThresholdBinaryInv(new Gray(p.I.Y), gMax);
                var vT = vTMin & vTMax;
                var t = hT & sT & vT;
                hTMin?.Dispose(); hTMax?.Dispose(); hT?.Dispose();
                sTMin?.Dispose(); sTMax?.Dispose(); sT?.Dispose();
                vTMin?.Dispose(); vTMax?.Dispose(); vT?.Dispose();
                //return t.Bytes;
                opening = new Image<Gray, byte>(t.Size);
                closing = new Image<Gray, byte>(t.Size);
                CvInvoke.MorphologyEx(t, opening, MorphOp.Open, _kernel5x5, new Point(1, 1), 1, BorderType.Default, new MCvScalar(0));
                CvInvoke.MorphologyEx(opening, closing, MorphOp.Close, _kernel11x11, new Point(1, 1), 1, BorderType.Default, new MCvScalar(0));
                r = closing.ThresholdBinary(new Gray(1), new Gray(255));
                t?.Dispose();
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
