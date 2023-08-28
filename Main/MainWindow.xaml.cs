using Main.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel => DataContext as MainViewModel;
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            viewModel.Initialize();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.Release();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(image);
            var pixelWidth = image.Source.Width;
            var pixelHeight = image.Source.Height;
            viewModel.X = Convert.ToInt32(pixelWidth * p.X / image.ActualWidth);
            viewModel.Y = Convert.ToInt32(pixelHeight * p.Y / image.ActualHeight);
        }
    }
}
