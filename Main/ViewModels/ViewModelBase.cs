using Main.Models;
using Basler.Vision;

namespace Main.ViewModels
{
    public class ViewModelBase : NotifyPropertyBase
    {
        public static VisionHandler Vision => VisionHandler.Instance;
    }
}
