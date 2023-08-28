using Basler.Base.Vision;
using Basler.Vision.Algorithm;

namespace Basler.Vision
{
    public class VisionHandler
    {
        private static VisionHandler _instance;
        public static VisionHandler Instance => _instance ?? (_instance = new VisionHandler());
        internal VisionHandler()
        {
            HSIClass = new HSIClassImple();
        }
        public HSIClassImple HSIClass { get; }
    }
}
