using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Main.Helper;
using Main.Models;
using Basler.Text;
using Basler.Base.Data;
using Basler.Vision;
using System.Threading;
using static Basler.Vision.SiSo;
using System.Diagnostics;
using System.Windows.Media;
using PixelFormat = Basler.Base.PixelFormat;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using Basler.Base;
using Point = System.Drawing.Point;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Media3D;
using System.Drawing.Drawing2D;

namespace Main.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private SiSo _Siso;
        private CancellationTokenSource _cancelToken;
        private bool _isTaskRunning;
        private string[] _processByDeviceMsg;
        private WriteableBitmap _wbmp;
        private Int32Rect _rect;
        private System.Drawing.Size _size;
        private int _updateCount;
        private Task[] _process;
        private Task _tDisp;
        private int _p;
        private int _FPSCount;
        private DateTime _now;
        private PerformanceCounter _CPUUsage;
        private LogHandler Log => LogHandler.Instance;
        private ConfigHandler Config => ConfigHandler.Instance;
        private SystemData SystemData => Config.SystemData;
        public MainViewModel()
        {
            DisplayItems = new ObservableCollection<ResultSelection>()
            {
                ResultSelection.Result,
                ResultSelection.Hue, 
                ResultSelection.Saturation,
                ResultSelection.Intensity,
                ResultSelection.Red,
                ResultSelection.Green,
                ResultSelection.Blue,
            };
            DebayerFormatItems = new ObservableCollection<PixelFormat>()
            {
                PixelFormat.GR,
                PixelFormat.GB,
                PixelFormat.BG,
                PixelFormat.RG
            };
            DeviceItems = new ObservableCollection<Device>()
            {
                Device.FPGA,
                Device.CPU
            };
            LogMessage = new ObservableCollection<string>();
            _processByDeviceMsg = new string[] 
            { "Debayer + Color Classification by FPGA", 
                "Debayer + Color Classification by CPU" };
            EnableDisplayImage = true;
            _process = new Task[5];
            _CPUUsage = new PerformanceCounter("Process", "% Processor Time", "BaslerDemo", true);
        }
        private int _hueMin;
        public int HueMin
        {
            get => _hueMin;
            set 
            { 
                SetProperty(ref _hueMin, CheckHSIMinMax(value));
                if(_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_HueMin", Convert.ToInt32(value));
                }
                ColorParam.Hue = new Point(_hueMin, ColorParam.Hue.Y);
            }
        }
        private int _hueMax;
        public int HueMax
        {
            get => _hueMax;
            set 
            { 
                SetProperty(ref _hueMax, CheckHSIMinMax(value));
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_HueMax", Convert.ToInt32(value));
                }
                ColorParam.Hue = new Point(ColorParam.Hue.X, _hueMax);
            }
        }
        private int _saturationMin;
        public int SaturationMin
        {
            get => _saturationMin;
            set 
            { 
                SetProperty(ref _saturationMin, CheckHSIMinMax(value));
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_SaturationMin", Convert.ToInt32(value));
                }
                ColorParam.Saturation = new Point(_saturationMin, ColorParam.Saturation.Y);
            }
        }
        private int _saturationMax;
        public int SaturationMax
        {
            get => _saturationMax;
            set 
            { 
                SetProperty(ref _saturationMax, CheckHSIMinMax(value));
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_SaturationMax", Convert.ToInt32(value));
                }
                ColorParam.Saturation = new Point(ColorParam.Saturation.X, _saturationMax);
            }
        }
        private int _intensityMin;
        public int IntensityMin
        {
            get => _intensityMin;
            set 
            { 
                SetProperty(ref _intensityMin, CheckHSIMinMax(value));
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_IntensityMin", Convert.ToInt32(value));
                }
                ColorParam.Intensity = new Point(_intensityMin, ColorParam.Intensity.Y);
            }
        }
        private int _intensityMax;
        public int IntensityMax
        {
            get => _intensityMax;
            set 
            { 
                SetProperty(ref _intensityMax, CheckHSIMinMax(value));
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_IntensityMax", Convert.ToInt32(value));
                }
                ColorParam.Intensity = new Point(ColorParam.Intensity.X, _intensityMax);
            }
        }
        private PixelFormat _selectDebayerFormatItem;
        public PixelFormat SelectDebayerFormatItem
        {
            get => _selectDebayerFormatItem;
            set 
            { 
                SetProperty(ref _selectDebayerFormatItem, value);
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_Debayer", Convert.ToInt32(value));
                }
            }
        }
        private ObservableCollection<PixelFormat> _debayerFormatItems;
        public ObservableCollection<PixelFormat> DebayerFormatItems
        {
            get => _debayerFormatItems;
            set => SetProperty(ref _debayerFormatItems, value);
        }
        private ResultSelection _selectDisplayItem;
        public ResultSelection SelectDisplayItem
        {
            get => _selectDisplayItem;
            set 
            { 
                SetProperty(ref _selectDisplayItem, value);
                if (_Siso != null && SelectDeviceItem == Device.FPGA)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_Display", Convert.ToInt32(value));
                }
            }
        }
        private ObservableCollection<ResultSelection> _displayItems;
        public ObservableCollection<ResultSelection> DisplayItems
        {
            get => _displayItems;
            set => SetProperty(ref _displayItems, value);
        }
        private Device _selectDeviceItem;
        public Device SelectDeviceItem
        {
            get => _selectDeviceItem;
            set 
            {
                SetProperty(ref _selectDeviceItem, value);
                if (value == Device.CPU)
                {
                    _Siso.SetParameter("Device1_Process0_Parameters_Display", Convert.ToInt32(7));
                }
                else
                {
                    SelectDisplayItem = ResultSelection.Result;
                }
                
                if(ProcessByDevice != _processByDeviceMsg[(int)value])
                {
                    ProcessByDevice = _processByDeviceMsg[(int)value];
                }
                HueMin = ColorParam.Hue.X;
                HueMax = ColorParam.Hue.Y;
                SaturationMin = ColorParam.Saturation.X;
                SaturationMax = ColorParam.Saturation.Y;
                IntensityMin = ColorParam.Intensity.X;
                IntensityMax = ColorParam.Intensity.Y;
            }
        }
        private ColorClassParam ColorParam 
        {
            get 
            {
                if (SelectDeviceItem == Device.FPGA)
                {
                    return SystemData.ColorFPGA;
                }
                else
                {
                    return SystemData.ColorCPU;
                }
            } 
        }
        private ObservableCollection<Device> _deviceItems;
        public ObservableCollection<Device> DeviceItems
        {
            get => _deviceItems;
            set => SetProperty(ref _deviceItems, value);
        }
        private ObservableCollection<string> _logMessage;
        public ObservableCollection<string> LogMessage
        {
            get => _logMessage;
            set => SetProperty(ref _logMessage, value);
        }
        private bool _enableDisplayImage;
        public bool EnableDisplayImage
        {
            get => _enableDisplayImage;
            set => SetProperty(ref _enableDisplayImage, value);
        }
        private string _processByDevice;
        public string ProcessByDevice
        {
            get => _processByDevice;
            set => SetProperty(ref _processByDevice, value);
        }
        private string _fps;
        public string Fps
        {
            get => _fps;
            set => SetProperty(ref _fps, value);
        }
        private string _xy;
        public string Xy
        {
            get => _xy;
            set => SetProperty(ref _xy, value);
        }
        public int X { get; set; }
        public int Y { get; set; }
        private ImageSource _displayImage;
        public ImageSource DisplayImage
        {
            get => _displayImage;
            set => SetProperty(ref _displayImage, value);
        }
        public byte[] ImageByte { get; set; }
        private string _CpuUsag;
        public string CPUUsage
        {
            get => _CpuUsag;
            set => SetProperty(ref _CpuUsag, value);
        }
        private string _Fps;
        public string FPS
        {
            get => _Fps;
            set => SetProperty(ref _Fps, value);
        }
        public void Initialize()
        {
            Config?.LoadSystemFile();
            FPGAInitialize();
            SelectDeviceItem = SystemData.SelectDevice;
            SelectDebayerFormatItem = SystemData.Format;
            SelectDisplayItem = SystemData.Result;
            EnableDisplayImage = SystemData.EnableDisplayImage;
        }
        public void Release()
        {
            SystemData.SelectDevice = SelectDeviceItem ;
            SystemData.Format = SelectDebayerFormatItem;
            SystemData.Result = SelectDisplayItem;
            SystemData.EnableDisplayImage = EnableDisplayImage;
            Config.SaveSystemFile();            
            FPGARelease();
        }
        public void FPGAInitialize()
        {
            _size = new System.Drawing.Size(4992, 5120);
            _Siso = new SiSo("Color_1Channel_IWCP.hap", 0, _size.Width, _size.Height, 1);
            _Siso.Initialize();

            _Siso.SetParameter("Device1_Process0_Camera_ResetStatus", (int)ResetStatus.Off);
            _Siso.SetParameter("Device1_Process0_AppletProperties_ExtensionGpioType", (int)GpioType.OpenDrain);
            _Siso.SetParameter("Device1_Process0_DRAM_XOffset", Convert.ToUInt32(0));
            _Siso.SetParameter("Device1_Process0_DRAM_XLength", Convert.ToUInt32(_size.Width));
            _Siso.SetParameter("Device1_Process0_DRAM_YOffset", Convert.ToUInt32(0));
            _Siso.SetParameter("Device1_Process0_DRAM_YLength", Convert.ToUInt32(_size.Height));

            _Siso.SetParameter("Device1_Process0_Parameters_ImageWidth", Convert.ToInt64(_size.Width));
            _Siso.SetParameter("Device1_Process0_Parameters_ImageHeight", Convert.ToInt64(_size.Height));


            _Siso.BoardInitialize();
            _Siso.StartAcquisition();
            _isTaskRunning = true;
            _cancelToken = new CancellationTokenSource();
            _wbmp = new WriteableBitmap(1248, 1280, 96, 96, PixelFormats.Gray8, null);
            _rect = new Int32Rect(0,0, (int)_wbmp.Width, (int)_wbmp.Height);
            DisplayImage = _wbmp;
            _now = DateTime.Now;
            Task.Factory.StartNew(() => {
                while(_isTaskRunning)
                {
                    if(_cancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    //var now1 = DateTime.Now;
                    //var byteImg = SelectDeviceItem == Device.FPGA ? _Siso.GetImage(0) :
                    //CPUProcess(_Siso.GetImage(0));
                    if(SelectDeviceItem == Device.FPGA)
                    {
                        FPGATask(_Siso.GetImage(0));
                    }
                    else
                    {
                        CPUTask(_Siso.GetImage(0));
                    }
                    
                    //var msImg = (DateTime.Now - now1).TotalMilliseconds;
                    
                    //fps++;
                    //index++;
                    //var ms = (DateTime.Now - now).TotalMilliseconds;
                    //now1 = DateTime.Now;
                    //ImageByte = byteImg;
                    //Task.Run(() => {
                    //    Refresh(byteImg);
                    //});                    
                    //var msUI = (DateTime.Now - now1).TotalMilliseconds;
                    //if (ms >= 1000)
                    //{
                    //    fps = fps - 2;
                    //    pct = myAppCpu.NextValue() / Environment.ProcessorCount;
                    //    Fps = $"FPS: {fps:##}. CPU={pct:##}%";
                    //    now = DateTime.Now;
                    //    Log.Info($"{index},fps,{fps:##},CPU,{pct:##}");
                    //    fps = 0;
                    //}
                    ////pct1 = Convert.ToInt32(myAppCpu.NextValue() / Environment.ProcessorCount);
                    //Log.Debug($"{index},Image,{msImg:##},UI,{msUI:##}");
                }
            }, _cancelToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        
        public void FPGARelease()
        {
            _cancelToken?.Cancel();
            _Siso?.ReleaseAll();
        }

        private void FPGATask(IntPtr img)
        {
            RefreshAsync(img);
        }

        private void CPUTask(IntPtr img)
        {
            while(_process[_p] != null && !_process[_p].IsCompleted)
            {
                _p++;
                _p = _p >= _process.Length ? 0 : _p;
                Task.Delay(1).Wait();
            }
            _process[_p] = Task.Run(() => {
                var result = CPUProcess(img);
                RefreshAsync(result);
            });
        }

        private void RefreshAsync(IntPtr img)
        {
            _FPSCount++;
            if((DateTime.Now - _now).TotalMilliseconds >= 1000 )
            {
                _now = DateTime.Now;
                var pct = _CPUUsage.NextValue() / Environment.ProcessorCount;
                FPS = $"{_FPSCount:##}";
                CPUUsage = $"{pct:##}%";
                Fps = $"FPS: {_FPSCount:##}. CPU={pct:##}%";
                _FPSCount = 0;
            }
            if ((_tDisp != null && !_tDisp.IsCompleted) || !EnableDisplayImage)
            {
                return;
            }
            _tDisp = Task.Run(() => {
                var simg = VisionHelper.Resize(img, _size.Width, _size.Height, _rect.Width, _rect.Height);
                Refresh(simg);
            });
        }

        private void RefreshAsync(byte[] img)
        {
            _FPSCount++;
            if ((DateTime.Now - _now).TotalMilliseconds >= 1000)
            {
                _now = DateTime.Now;
                var pct = _CPUUsage.NextValue() / Environment.ProcessorCount;
                FPS = $"{_FPSCount:##}";
                CPUUsage = $"{pct:##}%";
                Fps = $"FPS: {_FPSCount:##}. CPU={pct:##}%";
                _FPSCount = 0;
            }
            if ((_tDisp != null && !_tDisp.IsCompleted) || !EnableDisplayImage)
            {
                return;
            }
            
            _tDisp = Task.Run(() => {
                var ptr = Marshal.AllocHGlobal(img.Length);
                Marshal.Copy(img, 0, ptr, img.Length);
                var simg = VisionHelper.Resize(ptr, _size.Width, _size.Height, _rect.Width, _rect.Height);
                Marshal.FreeHGlobal(ptr);
                Refresh(simg);
            });
        }

        private int CheckHSIMinMax(int value)
        {
            if(value <= 0)
            {
                return 0;
            }
            if (value >= 255)
            {
                return 255;
            }
            return value;
        }

        void CreateThumbnail(string filename, BitmapSource image5)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image5));
                    encoder5.Save(stream5);
                }
            }
        }
        private void Refresh(byte[] img)
        {
            if (!EnableDisplayImage)
            {
                return;
            }
            ShowRGB(img);
            
            this.DispatchToUi(() =>
            {
                _wbmp.Lock();
                Marshal.Copy(img, 0, _wbmp.BackBuffer, img.Length);
                _wbmp.AddDirtyRect(_rect);
                _wbmp.Unlock();
                if (img == null)
                {
                    CreateThumbnail(@"C:\Temp\fpga_bgr.png", _wbmp.Clone());
                }
            });
        }
        private void ShowRGB(byte[] img)
        {
            try
            {
                var x = X;
                var y = Y;
                var w = _rect.Width;
                var h = _rect.Height;
                _updateCount = (_updateCount + 1) % 1;
                if (x >= 0 && x <= _size.Width && y >= 0 && y <= _size.Height && _updateCount == 0 && 
                    (SelectDisplayItem == ResultSelection.Hue ||
                     SelectDisplayItem == ResultSelection.Saturation ||
                     SelectDisplayItem == ResultSelection.Intensity))
                {
                    //var stride = y * w * 3;
                    //var x3 = x * 3;
                    //var r = SelectDeviceItem == Device.CPU ? img[stride + x3]: img[stride + x3+ 2];
                    //var g = SelectDeviceItem == Device.CPU ? img[stride + x3 + 1] : img[stride + x3 + 1];
                    //var b = SelectDeviceItem == Device.CPU ? img[stride + x3 + 2] : img[stride + x3 ];
                    var stride = y * w;
                    var g = SelectDeviceItem == Device.CPU ? img[stride + x] : img[stride + x];
                    var hsi = SelectDisplayItem == ResultSelection.Hue ? "Hue" : SelectDisplayItem == ResultSelection.Saturation ? "Saturation" : "Intensity";
                    Xy = $"X,Y = {x:#}, {y:#}\r{hsi} = {g}";
                }
                else
                {
                    Xy = "";
                }
            }
            catch
            {

            }
        }
        private byte[] CPUProcess(IntPtr img)
        {
            return VisionHandler.Instance.HSIClass.Execute((img, _size.Width, _size.Height), (SelectDisplayItem, SelectDebayerFormatItem,
                new Point(HueMin, HueMax), new Point(SaturationMin, SaturationMax), new Point(IntensityMin, IntensityMax)));
        }
        private void AddLog(string msg)
        {
            this.DispatchToUi(() =>
            {
                if (LogMessage.Count > 50)
                {
                    LogMessage.RemoveAt(LogMessage.Count - 1);
                }
                if (LogMessage.Count > 0)
                    LogMessage.Insert(0, msg);
                else
                    LogMessage.Add(msg);
                OnPropertyChanged("LogMessage");
            });
        }
    }
}
