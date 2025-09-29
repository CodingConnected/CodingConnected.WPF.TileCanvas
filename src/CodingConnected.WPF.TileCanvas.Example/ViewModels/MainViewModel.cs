using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System;
using CodingConnected.WPF.TileCanvas.Library.Enums;

namespace CodingConnected.WPF.TileCanvas.Example.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application managing the TileCanvas and panes
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private int _panelCounter = 0;
        private readonly Color[] _panelColors = {
            Colors.LightBlue, Colors.LightGreen, Colors.LightCoral,
            Colors.LightYellow, Colors.LightPink, Colors.LightCyan,
            Colors.LightGray, Colors.LightGoldenrodYellow
        };

        /// <summary>
        /// Collection of panes displayed in the TileCanvas
        /// </summary>
        public ObservableCollection<IPaneViewModel> Panes { get; }

        /// <summary>
        /// Whether the canvas is in edit mode
        /// </summary>
        [ObservableProperty]
        private EditMode _isEditMode = EditMode.Edit;

        /// <summary>
        /// Whether to show grid lines
        /// </summary>
        [ObservableProperty]
        private bool _showGrid = true;

        /// <summary>
        /// Whether to snap to grid when dragging
        /// </summary>
        [ObservableProperty]
        private bool _snapDrag = true;

        /// <summary>
        /// Whether to snap to grid when resizing
        /// </summary>
        [ObservableProperty]
        private bool _snapResize = true;

        /// <summary>
        /// Current grid mode (Fixed or Flexible)
        /// </summary>
        [ObservableProperty]
        private GridMode _gridMode = GridMode.Flexible;

        /// <summary>
        /// Number of columns for flexible grid mode
        /// </summary>
        [ObservableProperty]
        private int _columnCount = 7;

        /// <summary>
        /// Minium column width for flexible grid mode
        /// </summary>
        [ObservableProperty]
        private int _minColumnWidth = 100;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Panes = new ObservableCollection<IPaneViewModel>();
            
            // Initialize with default panes
            InitializeDefaultPanes();
        }

        /// <summary>
        /// Command to add a new chart pane
        /// </summary>
        [RelayCommand]
        private void AddChartPane()
        {
            var random = new Random();
            var color = _panelColors[random.Next(_panelColors.Length)];
            
            var chartPane = new ChartPaneViewModel($"Chart Panel {++_panelCounter}")
            {
                X = random.Next(0, 500),
                Y = random.Next(0, 300),
                HeaderColor = color.ToString()
            };
            
            Panes.Add(chartPane);
        }

        /// <summary>
        /// Command to add a new statistics pane
        /// </summary>
        [RelayCommand]
        private void AddStatsPane()
        {
            var random = new Random();
            var color = _panelColors[random.Next(_panelColors.Length)];
            
            var statsPane = new StatsPaneViewModel($"Statistics {++_panelCounter}")
            {
                X = random.Next(0, 500),
                Y = random.Next(0, 300),
                HeaderColor = color.ToString()
            };
            
            Panes.Add(statsPane);
        }

        /// <summary>
        /// Command to add a new table pane
        /// </summary>
        [RelayCommand]
        private void AddTablePane()
        {
            var random = new Random();
            var color = _panelColors[random.Next(_panelColors.Length)];
            
            var tablePane = new TablePaneViewModel($"Data Table {++_panelCounter}")
            {
                X = random.Next(0, 500),
                Y = random.Next(0, 300),
                HeaderColor = color.ToString()
            };
            
            Panes.Add(tablePane);
        }

        /// <summary>
        /// Command to add a generic pane (for testing)
        /// </summary>
        [RelayCommand]
        private void AddPane()
        {
            // Randomly choose which type of pane to add
            var random = new Random();
            var paneType = random.Next(0, 3);
            
            switch (paneType)
            {
                case 0:
                    AddChartPane();
                    break;
                case 1:
                    AddStatsPane();
                    break;
                case 2:
                    AddTablePane();
                    break;
            }
        }

        /// <summary>
        /// Command to clear all panes
        /// </summary>
        [RelayCommand]
        private void ClearAllPanes()
        {
            Panes.Clear();
            _panelCounter = 0;
        }

        /// <summary>
        /// Command to remove a specific pane
        /// </summary>
        /// <param name="pane">The pane to remove</param>
        [RelayCommand]
        private void RemovePane(IPaneViewModel? pane)
        {
            if (pane != null && Panes.Contains(pane))
            {
                Panes.Remove(pane);
            }
        }

        /// <summary>
        /// Initialize default panes to demonstrate the system
        /// </summary>
        private void InitializeDefaultPanes()
        {
            // Chart Panel
            var chartPane = new ChartPaneViewModel("Chart Panel")
            {
                X = 0,
                Y = 0,
                HeaderColor = Colors.LightBlue.ToString()
            };
            Panes.Add(chartPane);

            // Stats Panel
            var statsPane = new StatsPaneViewModel("Statistics")
            {
                X = 400,
                Y = 0,
                HeaderColor = Colors.LightGreen.ToString()
            };
            Panes.Add(statsPane);

            // Table Panel
            var tablePane = new TablePaneViewModel("Data Table")
            {
                X = 0,
                Y = 300,
                HeaderColor = Colors.LightCoral.ToString()
            };
            Panes.Add(tablePane);

            _panelCounter = 3;
        }

        /// <summary>
        /// Save layout functionality (placeholder for future implementation)
        /// </summary>
        [RelayCommand]
        private async Task SaveLayoutAsync()
        {
            // Implementation will be added when integrating with TileCanvas save functionality
            await Task.CompletedTask;
        }

        /// <summary>
        /// Load layout functionality (placeholder for future implementation)  
        /// </summary>
        [RelayCommand]
        private async Task LoadLayoutAsync()
        {
            // Implementation will be added when integrating with TileCanvas load functionality
            await Task.CompletedTask;
        }
    }
}