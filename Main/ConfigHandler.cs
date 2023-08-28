using Basler.Base;
using Basler.Base.Data;
using Basler.Vision;
using Main.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Main
{
    internal class ConfigHandler
    {
        private static ConfigHandler _instance;
        private static string SystemFile = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\System.xml";
        internal static ConfigHandler Instance => _instance ?? (_instance = new ConfigHandler());
        internal ConfigHandler()
        {
            SystemData = new SystemData();
        }
        internal SystemData SystemData { get; } 
        internal void SaveSystemFile()
        {
            var doc = new XmlDocument();
            if(File.Exists(SystemFile))
            {
                doc.Load(SystemFile);
            }
            else
            {
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));
            }
            var ndOldRoot = doc.DocumentElement;
            var ndNewRoot = doc.CreateElement("System");
            if (ndOldRoot == null)
                doc.AppendChild(ndNewRoot);
            else
                doc.ReplaceChild(ndNewRoot, ndOldRoot);

            var ndColor = doc.CreateElement("Color");
            XmlHelper.ReplaceOrCreate(ndNewRoot, ndColor);
            var ndFPGA = doc.CreateElement("FPGA");
            XmlHelper.ReplaceOrCreate(ndColor, ndFPGA);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndFPGA, "HueMin", SystemData.ColorFPGA.Hue.X);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndFPGA, "HueMax", SystemData.ColorFPGA.Hue.Y);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndFPGA, "SaturationMin", SystemData.ColorFPGA.Saturation.X);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndFPGA, "SaturationMax", SystemData.ColorFPGA.Saturation.Y);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndFPGA, "IntensityMin", SystemData.ColorFPGA.Intensity.X);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndFPGA, "IntensityMax", SystemData.ColorFPGA.Intensity.Y);
            var ndCPU = doc.CreateElement("CPU");
            XmlHelper.ReplaceOrCreate(ndColor, ndCPU);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndCPU, "HueMin", SystemData.ColorCPU.Hue.X);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndCPU, "HueMax", SystemData.ColorCPU.Hue.Y);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndCPU, "SaturationMin", SystemData.ColorCPU.Saturation.X);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndCPU, "SaturationMax", SystemData.ColorCPU.Saturation.Y);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndCPU, "IntensityMin", SystemData.ColorCPU.Intensity.X);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndCPU, "IntensityMax", SystemData.ColorCPU.Intensity.Y);

            var ndOthers = doc.CreateElement("Others");
            XmlHelper.ReplaceOrCreate(ndNewRoot, ndOthers);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndOthers, "ProcessBy", SystemData.SelectDevice);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndOthers, "PixelFormat", SystemData.Format);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndOthers, "ResultSelection", SystemData.Result);
            XmlHelper.ReplaceOrCreateAttribute(doc, ndOthers, "EnableDisplayImage", SystemData.EnableDisplayImage);

            doc.Save(SystemFile);
        }
        internal void LoadSystemFile()
        {
            var doc = new XmlDocument();
            if (File.Exists(SystemFile))
                doc.Load(SystemFile);
            else
                return;
            var ndRoot = doc.DocumentElement;
            var ndColor = ndRoot["Color"];
            if(ndColor != null)
            {
                var ndFPGA = ndColor["FPGA"];
                if (ndFPGA != null)
                {
                    var min = XmlHelper.ReadValueFromAttribute<int>(ndFPGA, "HueMin");
                    var max = XmlHelper.ReadValueFromAttribute<int>(ndFPGA, "HueMax");
                    SystemData.ColorFPGA.Hue = new Point(min, max);
                    min = XmlHelper.ReadValueFromAttribute<int>(ndFPGA, "SaturationMin");
                    max = XmlHelper.ReadValueFromAttribute<int>(ndFPGA, "SaturationMax");
                    SystemData.ColorFPGA.Saturation = new Point(min, max);
                    min = XmlHelper.ReadValueFromAttribute<int>(ndFPGA, "IntensityMin");
                    max = XmlHelper.ReadValueFromAttribute<int>(ndFPGA, "IntensityMax");
                    SystemData.ColorFPGA.Intensity = new Point(min, max);
                }
                var ndCPU = ndColor["CPU"];
                if (ndCPU != null)
                {
                    var min = XmlHelper.ReadValueFromAttribute<int>(ndCPU, "HueMin");
                    var max = XmlHelper.ReadValueFromAttribute<int>(ndCPU, "HueMax");
                    SystemData.ColorCPU.Hue = new Point(min, max);
                    min = XmlHelper.ReadValueFromAttribute<int>(ndCPU, "SaturationMin");
                    max = XmlHelper.ReadValueFromAttribute<int>(ndCPU, "SaturationMax");
                    SystemData.ColorCPU.Saturation = new Point(min, max);
                    min = XmlHelper.ReadValueFromAttribute<int>(ndCPU, "IntensityMin");
                    max = XmlHelper.ReadValueFromAttribute<int>(ndCPU, "IntensityMax");
                    SystemData.ColorCPU.Intensity = new Point(min, max);
                }
            }
            var ndOthers = ndRoot["Others"];
            if (ndOthers != null)
            {
                SystemData.SelectDevice = XmlHelper.ReadValueFromAttribute<Device>(ndOthers, "ProcessBy");
                SystemData.Format = XmlHelper.ReadValueFromAttribute<PixelFormat>(ndOthers, "PixelFormat");
                SystemData.Result = XmlHelper.ReadValueFromAttribute<ResultSelection>(ndOthers, "ResultSelection");
                SystemData.EnableDisplayImage = XmlHelper.ReadValueFromAttribute<bool>(ndOthers, "EnableDisplayImage");
            }
        }
    }
}
