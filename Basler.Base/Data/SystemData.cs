using System.Drawing;

namespace Basler.Base.Data
{
    public class SystemData
    {
        public SystemData() 
        {
            SelectDevice = Device.FPGA;
            ColorCPU = new ColorClassParam();
            ColorFPGA = new ColorClassParam();
            Format = PixelFormat.BG;
            Result = ResultSelection.Result;
            EnableDisplayImage = true;
        }
        public Device SelectDevice { get; set; }
        public ColorClassParam ColorCPU { get; set; }
        public ColorClassParam ColorFPGA { get; set; }
        public PixelFormat Format { get; set; }
        public ResultSelection Result { get; set;}
        public bool EnableDisplayImage { get; set; }
    }
    public class ColorClassParam
    {
        public ColorClassParam()
        {
            Hue = new Point(0, 255);
            Saturation = new Point(0, 255);
            Intensity = new Point(0, 255);
        }
        public Point Hue { get; set; }
        public Point Saturation { get; set; }
        public Point Intensity { get; set; }
    }
}
