using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodingConnected.WPF.TileCanvas.Library.Models;
using CodingConnected.WPF.TileCanvas.Library.Services;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;
using CodingConnected.WPF.TileCanvas.Example.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
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
        private readonly ILayoutSerializer _layoutSerializer;
        private readonly JsonSerializerOptions _enhancedSerializerOptions;
        private readonly GridCalculationService _gridCalculationService;
        
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
            _layoutSerializer = new JsonLayoutSerializer();
            _gridCalculationService = new GridCalculationService();
            
            // Initialize enhanced serializer options
            _enhancedSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            
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
        /// Save layout functionality
        /// </summary>
        [RelayCommand]
        private async Task SaveLayoutAsync()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON Layout Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = "layout.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var layoutFile = saveDialog.FileName;
                    var appDataFile = Path.ChangeExtension(layoutFile, ".appdata.json");
                    
                    // Save library layout data using existing serializer
                    var panelLayouts = ConvertViewModelsToPanelLayouts().ToList();
                    await _layoutSerializer.SaveToFileAsync(panelLayouts, layoutFile);
                    
                    // Save app-specific data separately
                    var appData = new AppData
                    {
                        ViewModelData = CreateViewModelDataDictionary(),
                        CanvasSettings = new AppCanvasSettings
                        {
                            PanelCounter = _panelCounter,
                            AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                            SavedAt = DateTime.Now,
                            GridMode = GridMode,
                            ShowGrid = ShowGrid,
                            SnapDrag = SnapDrag,
                            SnapResize = SnapResize,
                            ColumnCount = ColumnCount,
                            MinColumnWidth = MinColumnWidth,
                            IsEditMode = IsEditMode,
                            CanvasWidth = TileCanvas?.ActualWidth
                        }
                    };
                    
                    var appDataJson = JsonSerializer.Serialize(appData, _enhancedSerializerOptions);
                    await File.WriteAllTextAsync(appDataFile, appDataJson);
                    
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
        /// Load layout functionality
        /// </summary>
        [RelayCommand]
        private async Task LoadLayoutAsync()
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "JSON Layout Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var layoutFile = openDialog.FileName;
                    var appDataFile = Path.ChangeExtension(layoutFile, ".appdata.json");
                    
                    // Load library layout data
                    var layouts = await _layoutSerializer.LoadFromFileAsync(layoutFile);
                    
                    // Load app-specific data if it exists
                    Dictionary<string, ViewModelData>? viewModelData = null;
                    AppCanvasSettings? canvasSettings = null;
                    if (File.Exists(appDataFile))
                    {
                        var appDataJson = await File.ReadAllTextAsync(appDataFile);
                        var appData = JsonSerializer.Deserialize<AppData>(appDataJson, _enhancedSerializerOptions);
                        if (appData != null)
                        {
                            viewModelData = appData.ViewModelData;
                            canvasSettings = appData.CanvasSettings;
                            _panelCounter = appData.CanvasSettings.PanelCounter;
                        }
                    }
                    
                    // Restore grid configuration first if available
                    if (canvasSettings != null)
                    {
                        GridMode = canvasSettings.GridMode;
                        ShowGrid = canvasSettings.ShowGrid;
                        SnapDrag = canvasSettings.SnapDrag;
                        SnapResize = canvasSettings.SnapResize;
                        ColumnCount = canvasSettings.ColumnCount;
                        MinColumnWidth = canvasSettings.MinColumnWidth;
                        IsEditMode = canvasSettings.IsEditMode;
                    }
                    
                    // Reconstruct ViewModels from layout and app data
                    System.Diagnostics.Debug.WriteLine($"[Load] About to load panel layouts");
                    LoadFromPanelLayoutsWithAppData(layouts, viewModelData, canvasSettings);
                    
                    System.Diagnostics.Debug.WriteLine($"[Load] Finished loading panel layouts, checking positions:");
                    if (GridMode == GridMode.Flexible && TileCanvas != null)
                    {
                        // Use the same width calculation that TileCanvas uses internally
                        var loadCanvasWidth = TileCanvas.ActualWidth > 0 ? TileCanvas.ActualWidth : 1000;
                        
                        // Account for vertical scrollbar like TileCanvas.GetAvailableContentWidth() does
                        if (TileCanvas.GridMode == GridMode.Flexible && Panes.Any())
                        {
                            loadCanvasWidth -= System.Windows.SystemParameters.VerticalScrollBarWidth;
                        }
                        loadCanvasWidth = Math.Max(ColumnCount * MinColumnWidth, loadCanvasWidth);
                        var loadGridConfig = TileCanvas.Configuration?.Grid;
                        if (loadGridConfig != null)
                        {
                            var loadColumnWidths = _gridCalculationService.CalculateColumnWidths(loadGridConfig, loadCanvasWidth);
                            System.Diagnostics.Debug.WriteLine($"[Load] Current column widths: [{string.Join(", ", Array.ConvertAll(loadColumnWidths, x => x.ToString("F1")))}]");
                            
                            foreach (var pane in Panes)
                            {
                                // Calculate what the grid thinks this position should be
                                var expectedColumn = _gridCalculationService.CalculateStartColumn(pane.X, loadColumnWidths);
                                var expectedSpan = _gridCalculationService.CalculateColumnSpan(pane.Width, loadColumnWidths);
                                var expectedX = _gridCalculationService.CalculatePositionForColumn(expectedColumn, loadColumnWidths);
                                var expectedWidth = _gridCalculationService.CalculateWidthForColumnSpan(expectedSpan, loadColumnWidths);
                                
                                System.Diagnostics.Debug.WriteLine($"[Load] {pane.Title}: Loaded=({pane.X:F1}, {pane.Width:F1}) | Grid expects=({expectedX:F1}, {expectedWidth:F1}) | Col={expectedColumn}, Span={expectedSpan}");
                            }
                        }
                    }
                    else
                    {
                        foreach (var pane in Panes)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Load] After LoadFromPanelLayouts - {pane.Title}: X={pane.X:F1}, Width={pane.Width:F1}");
                        }
                    }
                    
                    // Apply post-load grid adjustments for flexible grid mode
                    System.Diagnostics.Debug.WriteLine($"[Load] About to apply post-load adjustments");
                    await ApplyPostLoadGridAdjustments();
                    
                    System.Diagnostics.Debug.WriteLine($"[Load] Finished post-load adjustments, final positions:");
                    foreach (var pane in Panes)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Load] Final - {pane.Title}: X={pane.X:F1}, Width={pane.Width:F1}");
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
        /// Converts ViewModels to PanelLayout objects for library serialization
        /// </summary>
        private IEnumerable<PanelLayout> ConvertViewModelsToPanelLayouts()
        {
            foreach (var pane in Panes)
            {
                var layout = new PanelLayout
                {
                    Id = pane.Id,
                    Title = pane.Title,
                    X = pane.X,
                    Y = pane.Y,
                    Width = pane.Width,
                    Height = pane.Height,
                    HeaderColor = pane.HeaderColor,
                    PanelType = pane.PaneType,
                    GridMode = GridMode,
                    ColumnCount = ColumnCount
                    // ContentData is not used since app-specific data is stored separately
                };
                
                // Calculate grid positioning for flexible mode
                System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: GridMode={GridMode}, TileCanvas={(TileCanvas != null ? "exists" : "null")}");
                
                if (GridMode == GridMode.Flexible && TileCanvas != null)
                {
                    try
                    {
                        // Ensure Configuration is synced before calculations
                        SyncConfigurationToTileCanvas();
                        
                        // Use the same width calculation that TileCanvas uses internally
                        // TileCanvas subtracts scrollbar width when there are panels, so we need to do the same
                        var canvasWidth = TileCanvas.ActualWidth > 0 ? TileCanvas.ActualWidth : 1000;
                        
                        // Account for vertical scrollbar like TileCanvas.GetAvailableContentWidth() does
                        if (TileCanvas.GridMode == GridMode.Flexible && Panes.Any())
                        {
                            canvasWidth -= System.Windows.SystemParameters.VerticalScrollBarWidth;
                        }
                        canvasWidth = Math.Max(ColumnCount * MinColumnWidth, canvasWidth);
                        var gridConfig = TileCanvas.Configuration?.Grid;
                        
                        System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: CanvasWidth={canvasWidth:F0}, GridConfig={(gridConfig != null ? "exists" : "null")}");
                        System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: TileCanvas.Configuration={(TileCanvas.Configuration != null ? "exists" : "null")}");
                        
                        if (gridConfig != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: GridConfig - Mode={gridConfig.Mode}, ColumnCount={gridConfig.ColumnCount}, MinWidth={gridConfig.MinColumnWidth}");
                            
                            var columnWidths = _gridCalculationService.CalculateColumnWidths(gridConfig, canvasWidth);
                            System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: Column widths calculated: [{string.Join(", ", Array.ConvertAll(columnWidths, x => x.ToString("F1")))}]");
                            
                            layout.GridColumn = _gridCalculationService.CalculateStartColumn(pane.X, columnWidths);
                            layout.GridColumnSpan = _gridCalculationService.CalculateColumnSpan(pane.Width, columnWidths);
                            layout.CanvasWidth = canvasWidth;
                            
                            System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: Pos({pane.X:F1}, {pane.Width:F1}) -> Grid(Col={layout.GridColumn}, Span={layout.GridColumnSpan}) CanvasWidth={canvasWidth:F0}");
                            
                            // Calculate the grid-aligned positions that should be saved
                            var gridAlignedX = _gridCalculationService.CalculatePositionForColumn(layout.GridColumn ?? 0, columnWidths);
                            var gridAlignedWidth = _gridCalculationService.CalculateWidthForColumnSpan(layout.GridColumnSpan ?? 1, columnWidths);
                            
                            // SAVE THE GRID-ALIGNED POSITIONS, not the original positions
                            layout.X = gridAlignedX;
                            layout.Width = gridAlignedWidth;
                            
                            System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: Saving grid-aligned positions: Original({pane.X:F1}, {pane.Width:F1}) -> GridAligned({gridAlignedX:F1}, {gridAlignedWidth:F1}) | Diff: ({Math.Abs(gridAlignedX - pane.X):F1}, {Math.Abs(gridAlignedWidth - pane.Width):F1})");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: GridConfig is null");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GridSave] Error calculating grid data for {pane.Title}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[GridSave] Stack trace: {ex.StackTrace}");
                        // If grid calculations fail, just use absolute positioning
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GridSave] {pane.Title}: Not flexible mode or no TileCanvas - using absolute positioning");
                }
                
                // Debug: Show final layout values
                System.Diagnostics.Debug.WriteLine($"[GridSave] Final layout for {pane.Title}: GridMode={layout.GridMode}, GridColumn={layout.GridColumn}, GridColumnSpan={layout.GridColumnSpan}, CanvasWidth={layout.CanvasWidth}");
                
                yield return layout;
            }
        }

        /// <summary>
        /// Repositions a pane based on current grid configuration if it was saved in flexible grid mode
        /// </summary>
        private void RepositionPaneForCurrentGrid(IPaneViewModel viewModel, PanelLayout layout, AppCanvasSettings? savedSettings)
        {
            // Only reposition if the layout was saved in flexible grid mode and we have grid positioning data
            if (layout.GridMode != GridMode.Flexible || !layout.GridColumn.HasValue || !layout.GridColumnSpan.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"[GridReposition] Skipping {viewModel.Title}: GridMode={layout.GridMode}, Column={layout.GridColumn}, Span={layout.GridColumnSpan}");
                return;
            }
            
            // If we're still in the same grid mode and configuration, check if repositioning is needed
            if (GridMode == GridMode.Flexible && TileCanvas != null)
            {
                try
                {
                    var currentCanvasWidth = TileCanvas.ActualWidth > 0 ? TileCanvas.ActualWidth : 1000;
                    var savedCanvasWidth = layout.CanvasWidth ?? savedSettings?.CanvasWidth ?? currentCanvasWidth;
                    var savedColumnCount = layout.ColumnCount ?? savedSettings?.ColumnCount ?? ColumnCount;
                    
                    System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: Current={currentCanvasWidth:F0}, Saved={savedCanvasWidth:F0}, CurrentCols={ColumnCount}, SavedCols={savedColumnCount}");
                    
                    // Only reposition if the canvas size or column configuration has changed significantly
                    var canvasSizeTolerance = Math.Max(20, savedCanvasWidth * 0.02); // 2% or minimum 20px
                    var canvasWidthDiff = Math.Abs(currentCanvasWidth - savedCanvasWidth);
                    
                    System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: Canvas size check - Diff: {canvasWidthDiff:F0}px, Tolerance: {canvasSizeTolerance:F0}px, Column change: {savedColumnCount != ColumnCount}");
                    
                    if (canvasWidthDiff > canvasSizeTolerance || savedColumnCount != ColumnCount)
                    {
                        var gridConfig = TileCanvas.Configuration?.Grid;
                        if (gridConfig != null)
                        {
                            // Calculate new column widths for current canvas size
                            var newColumnWidths = _gridCalculationService.CalculateColumnWidths(gridConfig, currentCanvasWidth);
                            
                            // Calculate new position and size based on saved grid position
                            var newX = _gridCalculationService.CalculatePositionForColumn(layout.GridColumn.Value, newColumnWidths);
                            var newWidth = _gridCalculationService.CalculateWidthForColumnSpan(layout.GridColumnSpan.Value, newColumnWidths);
                            
                            System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: Repositioning from ({viewModel.X:F0}, {viewModel.Width:F0}) to ({newX:F0}, {newWidth:F0}) [Col {layout.GridColumn.Value}, Span {layout.GridColumnSpan.Value}]");
                            
                            // Update the ViewModel position and size
                            System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: BEFORE update -> ({viewModel.X:F1}, {viewModel.Width:F1})");
                            viewModel.X = newX;
                            viewModel.Width = newWidth;
                            System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: AFTER update -> ({viewModel.X:F1}, {viewModel.Width:F1})");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: No repositioning needed (within tolerance)");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GridReposition] Error repositioning {viewModel.Title}: {ex.Message}");
                    // If repositioning fails, keep the original absolute positioning
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[GridReposition] {viewModel.Title}: Current mode is not flexible or no TileCanvas reference");
            }
        }
        
        /// <summary>
        /// Creates a dictionary of ViewModel-specific data keyed by pane ID
        /// </summary>
        private Dictionary<string, ViewModelData> CreateViewModelDataDictionary()
        {
            var dictionary = new Dictionary<string, ViewModelData>();
            
            foreach (var pane in Panes)
            {
                var viewModelData = new ViewModelData
                {
                    ViewModelType = pane.PaneType,
                    Properties = SerializeViewModelProperties(pane)
                };
                
                dictionary[pane.Id] = viewModelData;
            }
            
            return dictionary;
        }

        /// <summary>
        /// Loads ViewModels from PanelLayout objects with optional app-specific data
        /// </summary>
        private void LoadFromPanelLayoutsWithAppData(IEnumerable<PanelLayout> layouts, Dictionary<string, ViewModelData>? viewModelData, AppCanvasSettings? canvasSettings = null)
        {
            // Clear existing panes
            Panes.Clear();
            if (_panelCounter == 0) // Only reset if not already loaded from appData
                _panelCounter = 0;

            foreach (var layout in layouts)
            {
                var viewModel = CreateViewModelFromLayout(layout, viewModelData);
                if (viewModel != null)
                {
                    // SIMPLE: If we have grid column/span data, use it to calculate correct position/width for current canvas
                    if (GridMode == GridMode.Flexible && TileCanvas != null && layout.GridColumn.HasValue && layout.GridColumnSpan.HasValue)
                    {
                        RestoreGridPosition(viewModel, layout.GridColumn.Value, layout.GridColumnSpan.Value);
                    }
                    
                    Panes.Add(viewModel);
                    if (_panelCounter == 0) // Fallback counter extraction if not from appData
                        _panelCounter = Math.Max(_panelCounter, ExtractCounterFromTitle(layout.Title));
                }
            }
        }
        
        /// <summary>
        /// Restores a pane to the correct position based on its saved grid column and span
        /// </summary>
        private void RestoreGridPosition(IPaneViewModel viewModel, int savedColumn, int savedSpan)
        {
            try
            {
                SyncConfigurationToTileCanvas();
                var gridConfig = TileCanvas?.Configuration?.Grid;
                
                if (gridConfig != null)
                {
                    // Calculate current canvas width
                    var currentCanvasWidth = TileCanvas!.ActualWidth > 0 ? TileCanvas.ActualWidth : 1000;
                    //if (Panes.Any())
                    //{
                        currentCanvasWidth -= System.Windows.SystemParameters.VerticalScrollBarWidth;
                    //}
                    currentCanvasWidth = Math.Max(ColumnCount * MinColumnWidth, currentCanvasWidth);
                    
                    // Calculate current column widths
                    var columnWidths = _gridCalculationService.CalculateColumnWidths(gridConfig, currentCanvasWidth);
                    
                    // Calculate correct position and width for current canvas
                    var correctX = _gridCalculationService.CalculatePositionForColumn(savedColumn, columnWidths);
                    var correctWidth = _gridCalculationService.CalculateWidthForColumnSpan(savedSpan, columnWidths);
                    
                    System.Diagnostics.Debug.WriteLine($"[Restore] {viewModel.Title}: Saved=Col{savedColumn}/Span{savedSpan} -> Current=({correctX:F1}, {correctWidth:F1})");
                    
                    // Set the correct position and width
                    viewModel.X = correctX;
                    viewModel.Width = correctWidth;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Restore] Error restoring {viewModel.Title}: {ex.Message}");
            }
        }

        /// <summary>
        /// Serializes ViewModel-specific properties to JSON string
        /// </summary>
        private string? SerializeViewModelProperties(IPaneViewModel pane)
        {
            object? data = pane switch
            {
                ChartPaneViewModel chart => new
                {
                    chart.ChartType,
                    chart.DataDescription,
                    chart.SeriesInfo,
                    chart.ShowLegend
                },
                StatsPaneViewModel stats => new
                {
                    stats.HeaderText,
                    stats.RefreshInterval,
                    Statistics = stats.Statistics.Select(s => new
                    {
                        s.Label,
                        s.Value,
                        s.Icon,
                        s.TextColor
                    }).ToList()
                },
                TablePaneViewModel table => new
                {
                    table.TableDescription,
                    table.RowCount,
                    table.ColumnCount,
                    table.HasHeader,
                    table.DataSource
                },
                _ => null
            };
            
            return data != null ? JsonSerializer.Serialize(data, _enhancedSerializerOptions) : null;
        }

        /// <summary>
        /// Creates a ViewModel from a PanelLayout with optional app-specific data
        /// </summary>
        private IPaneViewModel? CreateViewModelFromLayout(PanelLayout layout, Dictionary<string, ViewModelData>? viewModelData = null)
        {
            IPaneViewModel? viewModel = layout.PanelType switch
            {
                "Chart" => new ChartPaneViewModel(layout.Title, layout.X, layout.Y),
                "Stats" => new StatsPaneViewModel(layout.Title, layout.X, layout.Y),
                "Table" => new TablePaneViewModel(layout.Title, layout.X, layout.Y, layout.Width, layout.Height),
                _ => null
            };

            if (viewModel != null)
            {
                // Set basic properties
                viewModel.Width = layout.Width;
                viewModel.Height = layout.Height;
                viewModel.HeaderColor = layout.HeaderColor;

                // Restore ViewModel-specific data from app data if available
                if (viewModelData != null && viewModelData.TryGetValue(layout.Id, out var appData))
                {
                    RestoreViewModelSpecificData(viewModel, appData.Properties);
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Restores ViewModel-specific data from JSON string
        /// </summary>
        private void RestoreViewModelSpecificData(IPaneViewModel viewModel, string? jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return;

            try
            {
                var jsonDocument = JsonDocument.Parse(jsonData);
                var jsonElement = jsonDocument.RootElement;
                
                switch (viewModel)
                {
                    case ChartPaneViewModel chart:
                        if (jsonElement.TryGetProperty("chartType", out var chartType))
                            chart.ChartType = chartType.GetString() ?? "Line";
                        if (jsonElement.TryGetProperty("dataDescription", out var dataDesc))
                            chart.DataDescription = dataDesc.GetString() ?? "Sample chart data";
                        if (jsonElement.TryGetProperty("seriesInfo", out var seriesInfo))
                            chart.SeriesInfo = seriesInfo.GetString() ?? "Chart data";
                        if (jsonElement.TryGetProperty("showLegend", out var showLegend))
                            chart.ShowLegend = showLegend.GetBoolean();
                        break;

                    case StatsPaneViewModel stats:
                        if (jsonElement.TryGetProperty("headerText", out var headerText))
                            stats.HeaderText = headerText.GetString() ?? "Key Metrics";
                        if (jsonElement.TryGetProperty("refreshInterval", out var refreshInterval))
                            stats.RefreshInterval = refreshInterval.GetInt32();
                        
                        // Restore statistics collection
                        if (jsonElement.TryGetProperty("statistics", out var statisticsArray))
                        {
                            stats.Statistics.Clear();
                            foreach (var statElement in statisticsArray.EnumerateArray())
                            {
                                if (statElement.TryGetProperty("label", out var label) &&
                                    statElement.TryGetProperty("value", out var value))
                                {
                                    var icon = statElement.TryGetProperty("icon", out var iconProp) ? iconProp.GetString() ?? "•" : "•";
                                    var textColor = statElement.TryGetProperty("textColor", out var colorProp) ? colorProp.GetString() ?? "#FF000000" : "#FF000000";
                                    
                                    stats.Statistics.Add(new StatisticItem(
                                        label.GetString() ?? "",
                                        value.GetString() ?? "",
                                        icon,
                                        textColor
                                    ));
                                }
                            }
                        }
                        break;

                    case TablePaneViewModel table:
                        if (jsonElement.TryGetProperty("tableDescription", out var tableDesc))
                            table.TableDescription = tableDesc.GetString() ?? "Data table";
                        if (jsonElement.TryGetProperty("rowCount", out var rowCount))
                            table.RowCount = rowCount.GetInt32();
                        if (jsonElement.TryGetProperty("columnCount", out var columnCount))
                            table.ColumnCount = columnCount.GetInt32();
                        if (jsonElement.TryGetProperty("hasHeader", out var hasHeader))
                            table.HasHeader = hasHeader.GetBoolean();
                        if (jsonElement.TryGetProperty("dataSource", out var dataSource))
                            table.DataSource = dataSource.GetString() ?? "Sample Dataset";
                        break;
                }
            }
            catch
            {
                // Ignore deserialization errors for ViewModel-specific data
                // The basic layout information is already restored
            }
        }
        
        /// <summary>
        /// Safety net - ensures all panes are grid-aligned after loading
        /// </summary>
        private async Task ApplyPostLoadGridAdjustments()
        {
            System.Diagnostics.Debug.WriteLine($"[PostLoad] Grid restoration should have been done during load - this is just a safety check");
            // The real work is now done in RestoreGridPosition() during load
            // This method can be simplified or removed
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