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
            DataContext = new MainViewModel();
        }

        // All panel creation and management is now handled by the MainViewModel
        // using MVVM data binding - no code-behind needed!
        // All functionality is now handled by MainViewModel and data binding!
    }
}