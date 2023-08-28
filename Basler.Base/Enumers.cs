using System;

namespace Basler.Base
{
    public enum PixelFormat
    {
        GB = 0,
        BG = 1,
        RG = 2,
        GR = 3
    }
    public enum ResultSelection
    {
        Result = 0,
        Hue = 1,
        Saturation = 2,
        Intensity = 3,
        Red = 4,
        Green = 5,
        Blue = 6
    }
    public enum Device
    {
        FPGA = 0,
        CPU
    }
}
