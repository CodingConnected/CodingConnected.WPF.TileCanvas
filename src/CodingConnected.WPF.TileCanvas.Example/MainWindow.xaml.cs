using System.Windows;
using CodingConnected.WPF.TileCanvas.Example.ViewModels;

namespace CodingConnected.WPF.TileCanvas.Example
{
    /// <summary>
    /// Main window using MVVM pattern with TileCanvas
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set the DataContext to MainViewModel for MVVM binding
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            
            // Set the TileCanvas reference for grid calculations
            Loaded += (s, e) => viewModel.TileCanvas = MainTileCanvas;
        }
    }
}