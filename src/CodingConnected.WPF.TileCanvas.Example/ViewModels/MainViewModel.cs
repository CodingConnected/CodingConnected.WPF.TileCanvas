using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System;
using CodingConnected.WPF.TileCanvas.Library.Enums;
using CodingConnected.WPF.TileCanvas.Library.Models;

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
        /// Reference to the TileCanvas control for grid calculations
        /// </summary>
        public CodingConnected.WPF.TileCanvas.Library.Controls.TileCanvas? TileCanvas 
        { 
            get => _tileCanvas;
            set
            {
                _tileCanvas = value;
                // Ensure Configuration.Grid is synced when TileCanvas is set
                SyncConfigurationToTileCanvas();
            }
        }
        private CodingConnected.WPF.TileCanvas.Library.Controls.TileCanvas? _tileCanvas;

        /// <summary>
        /// Collection of panes displayed in the TileCanvas
        /// </summary>
        public ObservableCollection<IPaneViewModel> Panes { get; }

        /// <summary>
        /// Whether the canvas is in edit mode
        /// </summary>
        [ObservableProperty]
        private bool _isEditMode = true;

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
        /// Visual spacing between panels in pixels
        /// </summary>
        [ObservableProperty]
        private double _panelSpacing = 5;
        
        /// <summary>
        /// Gap between panels in pixels
        /// </summary>
        [ObservableProperty]
        private double _panelGap = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Panes = [];
            
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
        /// Command to add a new table pane
        /// </summary>
        [RelayCommand]
        private void AddLabelPane()
        {
            var random = new Random();
            var color = _panelColors[random.Next(_panelColors.Length)];

            var tablePane = new LabelPaneViewModel($"Label {++_panelCounter}")
            {
                X = random.Next(0, 500),
                Y = random.Next(0, 300),
                HeaderColor = color.ToString(),
                ShowHeader = false,
                BorderThickness = 0
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
        /// Command to snap all panels to the nearest grid lines
        /// </summary>
        [RelayCommand]
        private void SnapPanels()
        {
            TileCanvas?.SnapAllPanelsToGrid();
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
        /// Save layout functionality - simplified ViewModel-only approach
        /// </summary>
        [RelayCommand]
        private async Task SaveLayoutAsync()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "ViewModel Layout Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = "layout.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var serializer = new CodingConnected.WPF.TileCanvas.Example.Services.ViewModelSerializer();
                    
                    // Create app settings from current state
                    var appSettings = new CodingConnected.WPF.TileCanvas.Example.Services.AppSettings
                    {
                        PanelCounter = _panelCounter,
                        GridMode = GridMode,
                        ShowGrid = ShowGrid,
                        SnapDrag = SnapDrag,
                        SnapResize = SnapResize,
                        ColumnCount = ColumnCount,
                        MinColumnWidth = MinColumnWidth,
                        IsEditMode = IsEditMode,
                        PanelSpacing = PanelSpacing,
                        PanelGap = PanelGap
                    };

                    // Save everything in a single file using ViewModels
                    await serializer.SaveAsync(Panes, appSettings, saveDialog.FileName);

                    MessageBox.Show("Layout saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving layout: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load layout functionality - simplified ViewModel-only approach
        /// </summary>
        [RelayCommand]
        private async Task LoadLayoutAsync()
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "ViewModel Layout Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var serializer = new CodingConnected.WPF.TileCanvas.Example.Services.ViewModelSerializer();
                    
                    // Load everything from the single ViewModel file
                    var (panes, appSettings) = await serializer.LoadAsync(openDialog.FileName);

                    // Apply app settings
                    _panelCounter = appSettings.PanelCounter;
                    GridMode = appSettings.GridMode;
                    ShowGrid = appSettings.ShowGrid;
                    SnapDrag = appSettings.SnapDrag;
                    SnapResize = appSettings.SnapResize;
                    ColumnCount = appSettings.ColumnCount;
                    MinColumnWidth = appSettings.MinColumnWidth;
                    IsEditMode = appSettings.IsEditMode;
                    PanelSpacing = appSettings.PanelSpacing;
                    PanelGap = appSettings.PanelGap;

                    // Replace the panes collection
                    Panes.Clear();
                    foreach (var pane in panes)
                    {
                        Panes.Add(pane);
                    }

                    MessageBox.Show("Layout loaded successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading layout: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        /// <summary>
        /// Synchronizes MainViewModel grid settings to TileCanvas Configuration
        /// </summary>
        private void SyncConfigurationToTileCanvas()
        {
            if (_tileCanvas != null)
            {
                if (_tileCanvas.Configuration == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ConfigSync] TileCanvas.Configuration is null, creating new one");
                    _tileCanvas.Configuration = new CanvasConfiguration();
                }
                
                if (_tileCanvas.Configuration.Grid == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ConfigSync] TileCanvas.Configuration.Grid is null, creating new one");
                    _tileCanvas.Configuration.Grid = new GridConfiguration();
                }
                
                var grid = _tileCanvas.Configuration.Grid;
                grid.Mode = GridMode;
                grid.ColumnCount = ColumnCount;
                grid.MinColumnWidth = MinColumnWidth;
                grid.ShowGrid = ShowGrid;
                grid.SnapToGridOnDrag = SnapDrag;
                grid.SnapToGridOnResize = SnapResize;
                
                System.Diagnostics.Debug.WriteLine($"[ConfigSync] Synced grid config: Mode={grid.Mode}, Columns={grid.ColumnCount}, MinWidth={grid.MinColumnWidth}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigSync] TileCanvas is null");
            }
        }
        
        /// <summary>
        /// Extracts counter number from panel title for maintaining counter state
        /// </summary>
        private int ExtractCounterFromTitle(string title)
        {
            var parts = title.Split(' ');
            foreach (var part in parts.Reverse())
            {
                if (int.TryParse(part, out int number))
                {
                    return number;
                }
            }
            return 0;
        }
    }
}