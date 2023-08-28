using System;
using System.Drawing;

namespace Basler.Base.Vision
{
    public interface IHSIClass
    {
        byte[] Execute((IntPtr bytes, int width, int height) image, (ResultSelection result, PixelFormat pxielFormat, Point H, Point S, Point I) p);
    }
}
