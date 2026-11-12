//using NexusMVVM.ViewModels;
using NexusWorkspace.ViewModels;
using System.Windows;

namespace NexusMVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}