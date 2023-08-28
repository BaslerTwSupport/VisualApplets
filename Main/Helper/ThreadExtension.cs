using System;
using System.Windows;
using System.Windows.Threading;

namespace Main.Helper
{
    public static class ThreadExtension
    {
        private static Dispatcher UiDispatcher => Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;

        public static void DispatchToUi(this object caller, Action method, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (UiDispatcher.CheckAccess())
            {
                method.Invoke();
            }
            else
            {
                UiDispatcher.Invoke(method, priority);
            }
        }
    }
}
