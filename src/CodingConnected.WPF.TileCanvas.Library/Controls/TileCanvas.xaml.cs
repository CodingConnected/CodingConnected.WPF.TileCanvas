using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CodingConnected.WPF.TileCanvas.Library.Enums;
using CodingConnected.WPF.TileCanvas.Library.Events;
using CodingConnected.WPF.TileCanvas.Library.Models;
using CodingConnected.WPF.TileCanvas.Library.Services;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;

namespace CodingConnected.WPF.TileCanvas.Library.Controls
{
    /// <summary>
    /// Main tile canvas control for creating draggable, resizable panel layouts
    /// </summary>
    public partial class TileCanvas : UserControl
    {
        #region Private Fields

        private Canvas? _gridLinesCanvas;
        private Canvas? _panelCanvas;
        private readonly GridCalculationService _gridService;
        private ILayoutSerializer _layoutSerializer;
        
        // MVVM support
        private readonly Dictionary<IPaneViewModel, TilePanel> _viewModelToPanelMap;
        private INotifyCollectionChanged? _currentItemsSource;

        // Dragging state
        private Point _dragStartPoint;
        private Point _dragStartElementPosition;
        private bool _isDragging;
        private TilePanel? _draggedElement;

        #endregion

        #region Dependency Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register(nameof(Configuration), typeof(CanvasConfiguration), typeof(TileCanvas),
                new PropertyMetadata(new CanvasConfiguration(), OnConfigurationChanged));

        public static readonly DependencyProperty MinCanvasWidthProperty =
            DependencyProperty.Register(nameof(MinCanvasWidth), typeof(double), typeof(TileCanvas),
                new PropertyMetadata(400.0));

        public static readonly DependencyProperty MinCanvasHeightProperty =
            DependencyProperty.Register(nameof(MinCanvasHeight), typeof(double), typeof(TileCanvas),
                new PropertyMetadata(400.0));

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(TileCanvas),
                new PropertyMetadata(7, OnColumnCountChanged));

        public static readonly DependencyProperty MinColumnWidthProperty =
            DependencyProperty.Register(nameof(MinColumnWidth), typeof(int), typeof(TileCanvas),
                new PropertyMetadata(100, OnMinColumnWidthChanged));


        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TileCanvas),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty PaneContentTemplateSelectorProperty =
            DependencyProperty.Register(nameof(PaneContentTemplateSelector), typeof(DataTemplateSelector), typeof(TileCanvas),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register(nameof(IsEditMode), typeof(bool), typeof(TileCanvas),
                new PropertyMetadata(true, OnIsEditModeChanged));

        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(TileCanvas),
                new PropertyMetadata(true, OnShowGridChanged));

        public static readonly DependencyProperty SnapToGridOnDragProperty =
            DependencyProperty.Register(nameof(SnapToGridOnDrag), typeof(bool), typeof(TileCanvas),
                new PropertyMetadata(true, OnSnapToGridOnDragChanged));

        public static readonly DependencyProperty SnapToGridOnResizeProperty =
            DependencyProperty.Register(nameof(SnapToGridOnResize), typeof(bool), typeof(TileCanvas),
                new PropertyMetadata(true, OnSnapToGridOnResizeChanged));

        public static readonly DependencyProperty GridModeProperty =
            DependencyProperty.Register(nameof(GridMode), typeof(GridMode), typeof(TileCanvas),
                new PropertyMetadata(GridMode.Fixed, OnGridModeChanged));

        public static readonly DependencyProperty PanelMarginProperty =
            DependencyProperty.Register(nameof(PanelMargin), typeof(double), typeof(TileCanvas),
                new PropertyMetadata(0.0, OnPanelMarginChanged));

        public static readonly DependencyProperty PanelSpacingProperty =
            DependencyProperty.Register(nameof(PanelSpacing), typeof(double), typeof(TileCanvas),
                new PropertyMetadata(0.0, OnPanelSpacingChanged));

        public static readonly DependencyProperty PanelGapProperty =
            DependencyProperty.Register(nameof(PanelGap), typeof(double), typeof(TileCanvas),
                new PropertyMetadata(0.0, OnPanelGapChanged));

        public static readonly DependencyProperty SelectedPaneIdProperty =
            DependencyProperty.Register(nameof(SelectedPaneId), typeof(string), typeof(TileCanvas),
                new PropertyMetadata(string.Empty, OnSelectedPaneIdChanged));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

        #region Properties

        /// <summary>
        /// Collection of panels on the canvas
        /// </summary>
        public ObservableCollection<TilePanel> Panels { get; }

        /// <summary>
        /// Configuration settings for the canvas
        /// </summary>
        public CanvasConfiguration Configuration
        {
            get => (CanvasConfiguration)GetValue(ConfigurationProperty);
            set => SetValue(ConfigurationProperty, value);
        }

        /// <summary>
        /// Minimum width of the canvas
        /// </summary>
        public double MinCanvasWidth
        {
            get => (double)GetValue(MinCanvasWidthProperty);
            set => SetValue(MinCanvasWidthProperty, value);
        }

        /// <summary>
        /// Minimum height of the canvas
        /// </summary>
        public double MinCanvasHeight
        {
            get => (double)GetValue(MinCanvasHeightProperty);
            set => SetValue(MinCanvasHeightProperty, value);
        }

        /// <summary>
        /// Layout serializer for save/load operations
        /// </summary>
        public ILayoutSerializer LayoutSerializer
        {
            get => _layoutSerializer;
            set => _layoutSerializer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Current grid mode
        /// </summary>
        public GridMode GridMode
        {
            get => (GridMode)GetValue(GridModeProperty);
            set => SetValue(GridModeProperty, value);
        }

        /// <summary>
        /// Current edit mode
        /// </summary>
        public bool IsEditMode
        {
            get => (bool)GetValue(IsEditModeProperty);
            set => SetValue(IsEditModeProperty, value);
        }

        /// <summary>
        /// Column count for flexible grid mode
        /// </summary>
        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        /// <summary>
        /// Minimum column width for flexible grid mode
        /// </summary>
        public int MinColumnWidth
        {
            get => (int)GetValue(MinColumnWidthProperty);
            set => SetValue(MinColumnWidthProperty, value);
        }

        /// <summary>
        /// Whether to show grid lines
        /// </summary>
        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        /// <summary>
        /// Whether to snap panels to grid when dragging
        /// </summary>
        public bool SnapToGridOnDrag
        {
            get => (bool)GetValue(SnapToGridOnDragProperty);
            set => SetValue(SnapToGridOnDragProperty, value);
        }

        /// <summary>
        /// Whether to snap panels to grid when resizing
        /// </summary>
        public bool SnapToGridOnResize
        {
            get => (bool)GetValue(SnapToGridOnResizeProperty);
            set => SetValue(SnapToGridOnResizeProperty, value);
        }

        /// <summary>
        /// Margin to account for in grid calculations (for panels that have margins)
        /// </summary>
        public double PanelMargin
        {
            get => (double)GetValue(PanelMarginProperty);
            set => SetValue(PanelMarginProperty, value);
        }

        /// <summary>
        /// Visual spacing between panels (applied as ContentMargin to each panel)
        /// </summary>
        public double PanelSpacing
        {
            get => (double)GetValue(PanelSpacingProperty);
            set => SetValue(PanelSpacingProperty, value);
        }

        /// <summary>
        /// Gap between panels (applied as PanelMargin to each panel)
        /// </summary>
        public double PanelGap
        {
            get => (double)GetValue(PanelGapProperty);
            set => SetValue(PanelGapProperty, value);
        }

        /// <summary>
        /// ItemsSource for MVVM binding - collection of IPaneViewModel objects
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// DataTemplateSelector for choosing appropriate content templates based on ViewModel type
        /// </summary>
        public DataTemplateSelector? PaneContentTemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(PaneContentTemplateSelectorProperty);
            set => SetValue(PaneContentTemplateSelectorProperty, value);
        }

        /// <summary>
        /// ID of the currently selected pane
        /// </summary>
        public string SelectedPaneId
        {
            get => (string)GetValue(SelectedPaneIdProperty);
            set => SetValue(SelectedPaneIdProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a panel is added to the canvas
        /// </summary>
        public event EventHandler<PanelEventArgs>? PanelAdded;

        /// <summary>
        /// Raised when a panel is removed from the canvas
        /// </summary>
        public event EventHandler<PanelEventArgs>? PanelRemoved;

        /// <summary>
        /// Raised when a panel is moved
        /// </summary>
        public event EventHandler<PanelEventArgs>? PanelMoved;

        /// <summary>
        /// Raised when a panel is resized
        /// </summary>
        public event EventHandler<PanelEventArgs>? PanelResized;

        /// <summary>
        /// Raised when the layout changes
        /// </summary>
        public event EventHandler<LayoutEventArgs>? LayoutChanged;

        /// <summary>
        /// Raised when a panel is selected
        /// </summary>
        public event EventHandler<PanelEventArgs>? PanelSelected;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCanvas()
        {
            InitializeComponent();
            _gridService = new GridCalculationService();
            _layoutSerializer = new JsonLayoutSerializer();
            Panels = [];
            _viewModelToPanelMap = [];

            // Sync the default panel margin with the grid service
            _gridService.DefaultPanelMargin = PanelMargin;

            Loaded += TileCanvas_Loaded;
            SizeChanged += TileCanvas_SizeChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a panel to the canvas at the specified position
        /// </summary>
        public void AddPanel(TilePanel panel, double x, double y)
        {
            ArgumentNullException.ThrowIfNull(panel);

            Point finalPosition = new(x, y);
            
            // In flexible mode with valid grid coordinates, use those instead of X/Y
            if (GridMode == GridMode.Flexible && panel.GridColumnSpan > 0)
            {
                var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());
                finalPosition.X = GridCalculationService.CalculatePositionForColumn(panel.GridColumn, columnWidths, PanelGap);
                finalPosition.Y = panel.GridRow * Configuration.Grid.GridSize;
                panel.Width = GridCalculationService.CalculateWidthForColumnSpan(panel.GridColumnSpan, columnWidths, PanelGap);
                panel.Height = panel.GridRowSpan * Configuration.Grid.GridSize;
            }
            else
            {
                // Apply grid snapping if enabled (for both modes)
                if (SnapToGridOnDrag)
                {
                    finalPosition = _gridService.SnapToGrid(finalPosition, Configuration.Grid, GetCanvasWidth());
                }

                // Apply constraints for flexible mode
                if (GridMode == GridMode.Flexible)
                {
                    var maxPosition = GetMaxAllowedPosition(panel);
                    finalPosition = new Point(
                        Math.Max(0, Math.Min(finalPosition.X, maxPosition.X)),
                        Math.Max(0, finalPosition.Y)
                    );
                }
                
                // Calculate grid coordinates from pixel positions
                if (GridMode == GridMode.Flexible)
                {
                    var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());
                    panel.GridColumn = GridCalculationService.CalculateStartColumn(finalPosition.X, columnWidths, PanelGap);
                    panel.GridColumnSpan = Math.Max(1, GridCalculationService.CalculateColumnSpan(panel.Width, columnWidths, PanelGap));
                    panel.GridRow = (int)Math.Round(finalPosition.Y / Configuration.Grid.GridSize);
                    panel.GridRowSpan = Math.Max(1, (int)Math.Round(panel.Height / Configuration.Grid.GridSize));
                }
            }

            Canvas.SetLeft(panel, finalPosition.X);
            Canvas.SetTop(panel, finalPosition.Y);

            Panels.Add(panel);
            _panelCanvas?.Children.Add(panel);

            SetupPanelEvents(panel);
            UpdatePanelEditMode(panel);
            panel.ContentMargin = new Thickness(PanelSpacing); 
            panel.PanelMargin = new Thickness(PanelGap);

            // Update canvas size to accommodate new panel
            UpdateCanvasSize();

            var panelLayout = panel.GetLayout();
            OnPanelAdded(new PanelEventArgs(panelLayout));
            OnLayoutChanged(LayoutChangeType.PanelAdded, [panelLayout]);
        }

        /// <summary>
        /// Removes a panel from the canvas
        /// </summary>
        public void RemovePanel(TilePanel panel)
        {
            if (panel == null) return;

            var panelLayout = panel.GetLayout();

            Panels.Remove(panel);
            _panelCanvas?.Children.Remove(panel);

            CleanupPanelEvents(panel);

            // Update canvas size after panel removal
            UpdateCanvasSize();

            OnPanelRemoved(new PanelEventArgs(panelLayout));
            OnLayoutChanged(LayoutChangeType.PanelRemoved, [panelLayout]);
        }

        /// <summary>
        /// Clears all panels from the canvas
        /// </summary>
        public void ClearPanels()
        {
            var allPanels = Panels.Select(p => p.GetLayout()).ToArray();

            foreach (var panel in Panels.ToArray())
            {
                CleanupPanelEvents(panel);
            }

            Panels.Clear();
            _panelCanvas?.Children.Clear();

            // Reset canvas size after clearing all panels
            UpdateCanvasSize();

            OnLayoutChanged(LayoutChangeType.LayoutCleared, allPanels);
        }

        /// <summary>
        /// Saves the current layout to a file
        /// </summary>
        public async Task SaveLayoutAsync(string filePath)
        {
            var layouts = Panels.Select(p => p.GetLayout()).ToArray();
            await LayoutSerializer.SaveToFileAsync(layouts, filePath);
        }

        /// <summary>
        /// Loads a layout from a file
        /// </summary>
        public async Task LoadLayoutAsync(string filePath)
        {
            var layouts = await LayoutSerializer.LoadFromFileAsync(filePath);
            LoadLayout(layouts);
        }

        /// <summary>
        /// Serializes the current layout to a string
        /// </summary>
        public string SerializeLayout()
        {
            var layouts = Panels.Select(p => p.GetLayout());
            return LayoutSerializer.Serialize(layouts);
        }

        /// <summary>
        /// Deserializes a layout from a string and applies it to the canvas
        /// </summary>
        public void DeserializeLayout(string layoutData)
        {
            var layouts = LayoutSerializer.Deserialize(layoutData);
            LoadLayout(layouts);
        }

        /// <summary>
        /// Refreshes the grid display
        /// </summary>
        public void RefreshGrid()
        {
            DrawGridLines();
        }

        /// <summary>
        /// Updates the canvas size based on mode and content
        /// </summary>
        public void UpdateCanvasSize()
        {
            double newMinWidth, newMinHeight;

            if (GridMode == GridMode.Flexible)
            {
                // In flexible mode, width is determined by column configuration
                newMinWidth = Configuration.Grid.ColumnCount * Configuration.Grid.MinColumnWidth;

                // Height is based on content (allow unlimited vertical scrolling)
                newMinHeight = CalculateRequiredHeight();
            }
            else
            {
                // In fixed mode, size is dynamic based on panel positions
                newMinWidth = GetDynamicCanvasWidth();
                newMinHeight = CalculateRequiredHeight();
            }

            MinCanvasWidth = newMinWidth;
            MinCanvasHeight = newMinHeight;

            // Update the panel canvas size to match
            if (_panelCanvas != null)
            {
                _panelCanvas.MinWidth = newMinWidth;
                _panelCanvas.MinHeight = newMinHeight;
            }

            // Refresh grid to match new size
            DrawGridLines();
        }

        /// <summary>
        /// Calculates the required height based on panel positions
        /// </summary>
        private double CalculateRequiredHeight()
        {
            if (Panels.Count == 0)
            {
                return 400; // Default minimum
            }

            double maxBottom = 0;
            foreach (var panel in Panels)
            {
                var top = Canvas.GetTop(panel);
                if (double.IsNaN(top)) top = 0;
                var bottom = top + Math.Max(panel.Height, panel.ActualHeight);
                maxBottom = Math.Max(maxBottom, bottom);
            }

            return Math.Max(400, maxBottom); // Add small padding
        }

        /// <summary>
        /// Gets dynamic canvas width based on panel positions (for fixed mode)
        /// </summary>
        private double GetDynamicCanvasWidth()
        {
            if (Panels.Count == 0)
            {
                return 400; // Default minimum
            }

            double maxRight = 0;
            foreach (var panel in Panels)
            {
                var left = Canvas.GetLeft(panel);
                if (double.IsNaN(left)) left = 0;
                var right = left + Math.Max(panel.Width, panel.ActualWidth);
                maxRight = Math.Max(maxRight, right);
            }

            return Math.Max(400, maxRight); // Add small padding
        }

        /// <summary>
        /// Finds the ScrollViewer parent in the visual tree
        /// </summary>
        private static ScrollViewer? FindParentScrollViewer(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;

            if (parent is ScrollViewer scrollViewer)
                return scrollViewer;

            return FindParentScrollViewer(parent);
        }

        /// <summary>
        /// Gets the maximum allowed position for a panel in flexible grid mode
        /// </summary>
        private Point GetMaxAllowedPosition(TilePanel panel)
        {
            if (GridMode != GridMode.Flexible)
            {
                // In fixed mode, allow unlimited dragging
                return new Point(double.MaxValue, double.MaxValue);
            }

            // In flexible mode, calculate the actual canvas width based on current available space
            var availableWidth = GetCanvasWidth();
            var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, availableWidth);
            var actualCanvasWidth = columnWidths.Sum();

            // Maximum X position is actual canvas width minus panel width
            var maxX = Math.Max(0, actualCanvasWidth - panel.ActualWidth);

            // For Y, we don't constrain in flexible mode (only X is column-based)
            var maxY = double.MaxValue;

            return new Point(maxX, maxY);
        }

        /// <summary>
        /// Snaps a position to the nearest grid point
        /// </summary>
        public Point SnapToGrid(Point position)
        {
            return _gridService.SnapToGrid(position, Configuration.Grid, GetCanvasWidth());
        }

        /// <summary>
        /// Snaps a size to valid grid dimensions
        /// </summary>
        public Size SnapSizeToGrid(Size size)
        {
            return _gridService.SnapSizeToGrid(size, Configuration.Grid, GetCanvasWidth());
        }

        /// <summary>
        /// Retrieve canvas width
        /// </summary>
        /// <returns>Canvas width</returns>
        public double GetCanvasWidth()
        {
            _panelCanvas = GetTemplateChild("PART_PanelCanvas") as Canvas;
            return _panelCanvas?.ActualWidth ?? 0;
        }

        /// <summary>
        /// Selects the panel with the specified ID
        /// </summary>
        /// <param name="panelId">ID of the panel to select</param>
        public void SelectPanel(string panelId)
        {
            if (string.IsNullOrEmpty(panelId))
            {
                // Clear selection
                SelectedPaneId = string.Empty;
                return;
            }

            // Find the panel
            var panel = Panels.FirstOrDefault(p => p.PanelId == panelId);
            if (panel != null)
            {
                // Update the selected pane ID
                SelectedPaneId = panelId;
                
                // Update all ViewModels' IsSelected property
                UpdateAllViewModelsSelection();
                
                var panelLayout = panel.GetLayout();
                OnPanelSelected(new PanelEventArgs(panelLayout));
            }
        }

        /// <summary>
        /// Snaps all panels to the nearest grid lines
        /// </summary>
        public void SnapAllPanelsToGrid()
        {
            foreach (var panel in Panels)
            {
                var currentPosition = new Point(
                    Canvas.GetLeft(panel), 
                    Canvas.GetTop(panel)
                );
                
                // Handle NaN values
                if (double.IsNaN(currentPosition.X)) currentPosition.X = 0;
                if (double.IsNaN(currentPosition.Y)) currentPosition.Y = 0;
                
                // Snap position to grid
                var snappedPosition = SnapToGrid(currentPosition);
                Canvas.SetLeft(panel, snappedPosition.X);
                Canvas.SetTop(panel, snappedPosition.Y);
                
                // Also snap size if resize snapping is enabled
                if (SnapToGridOnResize)
                {
                    var currentSize = new Size(panel.Width, panel.Height);
                    var snappedSize = SnapSizeToGrid(currentSize);
                    panel.Width = snappedSize.Width;
                    panel.Height = snappedSize.Height;
                }
                
                // Update corresponding ViewModel if this is a ViewModel-managed panel
                UpdateViewModelPosition(panel);
                if (SnapToGridOnResize)
                {
                    UpdateViewModelSize(panel);
                }
            }
            
            // Update canvas size after snapping all panels
            UpdateCanvasSize();
        }

        #endregion

        #region Template Parts

        /// <summary>
        /// Action to take when the control template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _gridLinesCanvas = GetTemplateChild("PART_GridLinesCanvas") as Canvas;
            _panelCanvas = GetTemplateChild("PART_PanelCanvas") as Canvas;

            if (_panelCanvas != null)
            {
                _panelCanvas.MouseMove += PanelCanvas_MouseMove;
                _panelCanvas.MouseUp += PanelCanvas_MouseUp;
            }

            RefreshGrid();
        }

        #endregion

        #region Event Handlers

        private void TileCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGrid();
            UpdateCanvasSize();
        }

        private void TileCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GridMode == GridMode.Flexible)
            {
                // Calculate old and new available widths for content
                var oldAvailableWidth = Math.Max(e.PreviousSize.Width - SystemParameters.VerticalScrollBarWidth,
                                                Configuration.Grid.ColumnCount * Configuration.Grid.MinColumnWidth);
                var newAvailableWidth = GetCanvasWidth();

                // Only update if there's a meaningful size change
                if (Math.Abs(oldAvailableWidth - newAvailableWidth) > 1)
                {
                    var oldColumnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, oldAvailableWidth);
                    UpdateCanvasSize();
                    RefreshGrid();
                    UpdatePanelPositionsForNewGrid(oldColumnWidths);
                }
            }
            else
            {
                // In fixed mode, just refresh the grid
                RefreshGrid();
            }
        }

        private void PanelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed && _draggedElement != null)
            {
                var currentPoint = e.GetPosition(_panelCanvas);
                var totalOffset = currentPoint - _dragStartPoint;

                var newLeft = _dragStartElementPosition.X + totalOffset.X;
                var newTop = _dragStartElementPosition.Y + totalOffset.Y;

                // Apply constraints based on grid mode
                if (GridMode == GridMode.Flexible)
                {
                    // In flexible mode, constrain to column boundaries
                    var maxPosition = GetMaxAllowedPosition(_draggedElement);
                    newLeft = Math.Max(0, Math.Min(newLeft, maxPosition.X));
                    newTop = Math.Max(0, newTop); // Only constrain top boundary, allow unlimited bottom
                }
                // In fixed mode, no constraints - allow dragging beyond canvas bounds to expand scrollable area

                // Apply snapping if enabled
                if (SnapToGridOnDrag)
                {
                    var snappedPosition = _gridService.SnapToGrid(new Point(newLeft, newTop), Configuration.Grid, GetCanvasWidth());
                    Canvas.SetLeft(_draggedElement, snappedPosition.X);
                    Canvas.SetTop(_draggedElement, snappedPosition.Y);
                }
                else
                {
                    // When snapping is disabled, use exact positioning
                    Canvas.SetLeft(_draggedElement, newLeft);
                    Canvas.SetTop(_draggedElement, newTop);
                }
            }
        }

        private void PanelCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && _draggedElement != null)
            {
                _isDragging = false;
                Panel.SetZIndex(_draggedElement, 0);

                // Update canvas size to accommodate new panel position
                UpdateCanvasSize();

                // Update corresponding ViewModel position if this is a ViewModel-managed panel
                UpdateViewModelPosition(_draggedElement);

                var panelLayout = _draggedElement.GetLayout();
                OnPanelMoved(new PanelEventArgs(panelLayout));
                OnLayoutChanged(LayoutChangeType.PanelMoved, [panelLayout]);

                _draggedElement = null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles ItemsSource property changes
        /// </summary>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
            }
        }

        /// <summary>
        /// Handles changes to the ItemsSource collection
        /// </summary>
        private void OnItemsSourceChanged(IEnumerable? _, IEnumerable? newValue)
        {
            // Unsubscribe from old collection
            if (_currentItemsSource != null)
            {
                _currentItemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
            }

            // Clear existing panels when ItemsSource changes
            ClearViewModelPanels();

            _currentItemsSource = newValue as INotifyCollectionChanged;

            // Subscribe to new collection
            if (_currentItemsSource != null)
            {
                _currentItemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            }

            // Add panels for new items
            if (newValue != null)
            {
                foreach (var item in newValue)
                {
                    if (item is IPaneViewModel viewModel)
                    {
                        AddPanelForViewModel(viewModel);
                    }
                }
            }
        }

        private static void OnColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas c)
            {
                c.Configuration.Grid.ColumnCount = (int)e.NewValue;
                c.UpdateCanvasSize();
                c.UpdateLayout();
            }
        }

        private static void OnMinColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas c)
            {
                c.Configuration.Grid.MinColumnWidth = (int)e.NewValue;
                c.UpdateCanvasSize();
                c.UpdateLayout();
            }
        }

        /// <summary>
        /// Handles collection changed events from ItemsSource
        /// </summary>
        private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is IPaneViewModel viewModel)
                            {
                                AddPanelForViewModel(viewModel);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is IPaneViewModel viewModel)
                            {
                                RemovePanelForViewModel(viewModel);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearViewModelPanels();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is IPaneViewModel viewModel)
                            {
                                RemovePanelForViewModel(viewModel);
                            }
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is IPaneViewModel viewModel)
                            {
                                AddPanelForViewModel(viewModel);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates and adds a TilePanel for the given ViewModel
        /// </summary>
        private void AddPanelForViewModel(IPaneViewModel viewModel)
        {
            if (_viewModelToPanelMap.ContainsKey(viewModel))
                return;

            var panel = CreateTilePanelFromViewModel(viewModel);
            _viewModelToPanelMap[viewModel] = panel;
            
            // Subscribe to ViewModel property changes to sync with TilePanel
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            AddPanel(panel, viewModel.X, viewModel.Y);
        }

        /// <summary>
        /// Removes the TilePanel associated with the given ViewModel
        /// </summary>
        private void RemovePanelForViewModel(IPaneViewModel viewModel)
        {
            if (_viewModelToPanelMap.TryGetValue(viewModel, out var panel))
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                _viewModelToPanelMap.Remove(viewModel);
                RemovePanel(panel);
            }
        }

        /// <summary>
        /// Clears all ViewModel-generated panels
        /// </summary>
        private void ClearViewModelPanels()
        {
            var viewModelsToRemove = _viewModelToPanelMap.Keys.ToArray();
            foreach (var viewModel in viewModelsToRemove)
            {
                RemovePanelForViewModel(viewModel);
            }
        }

        /// <summary>
        /// Creates a TilePanel from a ViewModel
        /// </summary>
        private TilePanel CreateTilePanelFromViewModel(IPaneViewModel viewModel)
        {
            var panel = new TilePanel
            {
                PanelId = viewModel.Id,
                Title = viewModel.Title,
                Width = viewModel.Width,
                Height = viewModel.Height,
                PanelType = viewModel.PaneType,
                HeaderVisibility = viewModel.ShowHeader ? Visibility.Visible : Visibility.Collapsed,
                GridColumn = viewModel.GridColumn,
                GridColumnSpan = viewModel.GridColumnSpan,
                GridRow = viewModel.GridRow,
                GridRowSpan = viewModel.GridRowSpan
            };

            // Set header color
            if (!string.IsNullOrEmpty(viewModel.HeaderColor))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(viewModel.HeaderColor);
                    panel.HeaderBrush = new SolidColorBrush(color);
                }
                catch
                {
                    // Use default if color parsing fails
                }
            }

            // Set background color
            if (!string.IsNullOrEmpty(viewModel.BackgroundColor))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(viewModel.BackgroundColor);
                    panel.PaneBackgroundBrush = new SolidColorBrush(color);
                }
                catch
                {
                    // Use default if color parsing fails
                }
            }

            // Set border color
            if (!string.IsNullOrEmpty(viewModel.BorderColor))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(viewModel.BorderColor);
                    panel.PaneBorderBrush = new SolidColorBrush(color);
                }
                catch
                {
                    // Use default if color parsing fails
                }
            }

            panel.PaneBorderThickness = new Thickness(viewModel.BorderThickness);

            // Set content using DataTemplateSelector if available
            if (PaneContentTemplateSelector != null)
            {
                var template = PaneContentTemplateSelector.SelectTemplate(viewModel, panel);
                if (template != null)
                {
                    var content = template.LoadContent();
                    if (content is FrameworkElement element)
                    {
                        element.DataContext = viewModel;
                        panel.Content = element;
                    }
                }
                else
                {
                    // Selector exists but returned null - use fallback
                    SetFallbackContent(panel, viewModel);
                }
            }
            else
            {
                // No selector provided - use WPF automatic template resolution
                var contentPresenter = new ContentPresenter
                {
                    Content = viewModel
                };
                panel.Content = contentPresenter;
                
                // WPF will automatically find DataTemplate with matching DataType
                // If no template is found, WPF will use default content presentation
            }

            return panel;
        }

        /// <summary>
        /// Updates the ViewModel size when a panel is resized
        /// </summary>
        private void UpdateViewModelSize(TilePanel panel)
        {
            // Find the ViewModel that corresponds to this panel
            var viewModelEntry = _viewModelToPanelMap.FirstOrDefault(kvp => kvp.Value == panel);
            if (viewModelEntry.Key != null)
            {
                var viewModel = viewModelEntry.Key;
                
                // Temporarily unsubscribe to avoid circular updates
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                
                // Update ViewModel size
                viewModel.Width = panel.Width;
                viewModel.Height = panel.Height;
                
                // In flexible mode, also update grid coordinates
                if (GridMode == GridMode.Flexible)
                {
                    var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());
                    viewModel.GridColumnSpan = GridCalculationService.CalculateColumnSpan(panel.Width, columnWidths, PanelGap);
                    viewModel.GridRowSpan = (int)Math.Round(panel.Height / Configuration.Grid.GridSize);
                    panel.GridColumnSpan = viewModel.GridColumnSpan;
                    panel.GridRowSpan = viewModel.GridRowSpan;
                }
                
                // Re-subscribe
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Updates the ViewModel position when a panel is moved
        /// </summary>
        private void UpdateViewModelPosition(TilePanel panel)
        {
            // Find the ViewModel that corresponds to this panel
            var viewModelEntry = _viewModelToPanelMap.FirstOrDefault(kvp => kvp.Value == panel);
            if (viewModelEntry.Key != null)
            {
                var viewModel = viewModelEntry.Key;
                var left = Canvas.GetLeft(panel);
                var top = Canvas.GetTop(panel);
                
                if (!double.IsNaN(left) && !double.IsNaN(top))
                {
                    // Temporarily unsubscribe to avoid circular updates
                    viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    
                    // Update ViewModel position
                    viewModel.X = left;
                    viewModel.Y = top;
                    
                    // In flexible mode, also update grid coordinates
                    if (GridMode == GridMode.Flexible)
                    {
                        var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());
                        viewModel.GridColumn = GridCalculationService.CalculateStartColumn(left, columnWidths, PanelGap);
                        viewModel.GridRow = (int)Math.Round(top / Configuration.Grid.GridSize);
                        panel.GridColumn = viewModel.GridColumn;
                        panel.GridRow = viewModel.GridRow;
                    }
                    
                    // Re-subscribe
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Handles property changes from ViewModels to sync with TilePanels
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is IPaneViewModel viewModel && _viewModelToPanelMap.TryGetValue(viewModel, out var panel))
            {
                switch (e.PropertyName)
                {
                    case nameof(IPaneViewModel.Title):
                        panel.Title = viewModel.Title;
                        break;
                    case nameof(IPaneViewModel.Width):
                        panel.Width = viewModel.Width;
                        break;
                    case nameof(IPaneViewModel.Height):
                        panel.Height = viewModel.Height;
                        break;
                    case nameof(IPaneViewModel.X):
                        Canvas.SetLeft(panel, viewModel.X);
                        UpdateCanvasSize();
                        break;
                    case nameof(IPaneViewModel.Y):
                        Canvas.SetTop(panel, viewModel.Y);
                        UpdateCanvasSize();
                        break;
                    case nameof(IPaneViewModel.HeaderColor):
                        if (!string.IsNullOrEmpty(viewModel.HeaderColor))
                        {
                            try
                            {
                                var color = (Color)ColorConverter.ConvertFromString(viewModel.HeaderColor);
                                panel.HeaderBrush = new SolidColorBrush(color);
                            }
                            catch
                            {
                                // Ignore invalid colors
                            }
                        }
                        break;
                    case nameof(IPaneViewModel.BackgroundColor):
                        if (!string.IsNullOrEmpty(viewModel.BackgroundColor))
                        {
                            try
                            {
                                var color = (Color)ColorConverter.ConvertFromString(viewModel.BackgroundColor);
                                panel.PaneBackgroundBrush = new SolidColorBrush(color);
                            }
                            catch
                            {
                                // Ignore invalid colors
                            }
                        }
                        break;
                    case nameof(IPaneViewModel.BorderColor):
                        if (!string.IsNullOrEmpty(viewModel.BorderColor))
                        {
                            try
                            {
                                var color = (Color)ColorConverter.ConvertFromString(viewModel.BorderColor);
                                panel.PaneBorderBrush = new SolidColorBrush(color);
                            }
                            catch
                            {
                                // Ignore invalid colors
                            }
                        }
                        break;
                    case nameof(IPaneViewModel.BorderThickness):
                        panel.PaneBorderThickness = new Thickness(viewModel.BorderThickness);
                        break;
                    case nameof(IPaneViewModel.ShowHeader):
                        panel.HeaderVisibility = viewModel.ShowHeader ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case nameof(IPaneViewModel.IsSelected):
                        if (viewModel.IsSelected)
                        {
                            // Update SelectedPaneId when a ViewModel is selected
                            SelectedPaneId = viewModel.Id;
                        }
                        else if (SelectedPaneId == viewModel.Id)
                        {
                            // Clear selection if this was the selected ViewModel
                            SelectedPaneId = string.Empty;
                        }
                        break;
                    case nameof(IPaneViewModel.GridColumn):
                        panel.GridColumn = viewModel.GridColumn;
                        if (GridMode == GridMode.Flexible)
                        {
                            var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());
                            var newLeft = GridCalculationService.CalculatePositionForColumn(viewModel.GridColumn, columnWidths, PanelGap);
                            Canvas.SetLeft(panel, newLeft);
                            UpdateCanvasSize();
                        }
                        break;
                    case nameof(IPaneViewModel.GridColumnSpan):
                        panel.GridColumnSpan = viewModel.GridColumnSpan;
                        if (GridMode == GridMode.Flexible)
                        {
                            var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());
                            panel.Width = GridCalculationService.CalculateWidthForColumnSpan(viewModel.GridColumnSpan, columnWidths, PanelGap);
                        }
                        break;
                    case nameof(IPaneViewModel.GridRow):
                        panel.GridRow = viewModel.GridRow;
                        if (GridMode == GridMode.Flexible)
                        {
                            var newTop = viewModel.GridRow * Configuration.Grid.GridSize;
                            Canvas.SetTop(panel, newTop);
                            UpdateCanvasSize();
                        }
                        break;
                    case nameof(IPaneViewModel.GridRowSpan):
                        panel.GridRowSpan = viewModel.GridRowSpan;
                        if (GridMode == GridMode.Flexible)
                        {
                            panel.Height = viewModel.GridRowSpan * Configuration.Grid.GridSize;
                        }
                        break;
                }
            }
        }

        private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.RefreshGrid();
                canvas.UpdateAllPanelsEditMode();
            }
        }

        private static void OnIsEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.Configuration.IsEditMode = (bool)e.NewValue;
                canvas.UpdateAllPanelsEditMode();
                canvas.OnLayoutChanged(LayoutChangeType.EditModeChanged, []);
            }
        }

        private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.Configuration.Grid.ShowGrid = (bool)e.NewValue;
                canvas.RefreshGrid();
            }
        }

        private static void OnSnapToGridOnDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.Configuration.Grid.SnapToGridOnDrag = (bool)e.NewValue;
            }
        }

        private static void OnSnapToGridOnResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.Configuration.Grid.SnapToGridOnResize = (bool)e.NewValue;
            }
        }

        private static void OnGridModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.Configuration.Grid.Mode = (GridMode)e.NewValue;
                canvas.RefreshGrid();
                canvas.OnLayoutChanged(LayoutChangeType.GridConfigurationChanged, []);
            }
        }

        private static void OnPanelMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas._gridService.DefaultPanelMargin = (double)e.NewValue;
                // Refresh grid to reflect new margin calculations
                canvas.RefreshGrid();
            }
        }

        private static void OnPanelSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.UpdateAllPanelsSpacing();
            }
        }

        private static void OnPanelGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.UpdateAllPanelsGap();
            }
        }

        private static void OnSelectedPaneIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TileCanvas canvas)
            {
                canvas.UpdatePanelSelection((string)e.NewValue);
            }
        }

        private void LoadLayout(IEnumerable<PanelLayout> layouts)
        {
            ClearPanels();

            foreach (var layout in layouts)
            {
                var panel = TilePanel.FromLayout(layout);
                AddPanel(panel, layout.X, layout.Y);
            }

            // Update canvas size to accommodate loaded layout
            UpdateCanvasSize();

            OnLayoutChanged(LayoutChangeType.LayoutLoaded, [.. layouts]);
        }

        private void SetupPanelEvents(TilePanel panel)
        {
            panel.DragStarted += Panel_DragStarted;
            panel.Resized += Panel_Resized;
            panel.CloseRequested += Panel_CloseRequested;
            panel.ColorChanged += Panel_ColorChanged;
            panel.TitleChanged += Panel_TitleChanged;
            panel.PanelSelected += Panel_PanelSelected;
        }

        private void CleanupPanelEvents(TilePanel panel)
        {
            panel.DragStarted -= Panel_DragStarted;
            panel.Resized -= Panel_Resized;
            panel.CloseRequested -= Panel_CloseRequested;
            panel.ColorChanged -= Panel_ColorChanged;
            panel.TitleChanged -= Panel_TitleChanged;
            panel.PanelSelected -= Panel_PanelSelected;
        }

        private void Panel_DragStarted(object? sender, MouseButtonEventArgs e)
        {
            if (!IsEditMode || sender is not TilePanel panel)
                return;

            _draggedElement = panel;
            _dragStartPoint = e.GetPosition(_panelCanvas);

            var currentLeft = Canvas.GetLeft(_draggedElement);
            var currentTop = Canvas.GetTop(_draggedElement);
            if (double.IsNaN(currentLeft)) currentLeft = 0;
            if (double.IsNaN(currentTop)) currentTop = 0;
            _dragStartElementPosition = new Point(currentLeft, currentTop);

            _isDragging = true;
            Panel.SetZIndex(_draggedElement, 999);
        }

        private void Panel_Resized(object? sender, EventArgs e)
        {
            if (sender is TilePanel panel)
            {
                // Update canvas size to accommodate resized panel
                UpdateCanvasSize();

                // Update corresponding ViewModel size if this is a ViewModel-managed panel
                UpdateViewModelSize(panel);

                var panelLayout = panel.GetLayout();
                OnPanelResized(new PanelEventArgs(panelLayout));
                OnLayoutChanged(LayoutChangeType.PanelResized, [panelLayout]);
            }
        }

        private void Panel_CloseRequested(object? sender, EventArgs e)
        {
            if (sender is TilePanel panel)
            {
                // Find the corresponding ViewModel and remove it from the ItemsSource collection
                var viewModelEntry = _viewModelToPanelMap.FirstOrDefault(kvp => kvp.Value == panel);
                if (viewModelEntry.Key != null)
                {
                    // Remove from ViewModel collection - this will trigger ItemsSource_CollectionChanged
                    // which will call RemovePanelForViewModel, which will call RemovePanel
                    if (ItemsSource is System.Collections.IList itemsList)
                    {
                        itemsList.Remove(viewModelEntry.Key);
                    }
                }
                else
                {
                    // Fallback: remove panel directly if no ViewModel mapping exists
                    RemovePanel(panel);
                }
            }
        }

        private void Panel_ColorChanged(object? sender, ColorChangedEventArgs e)
        {
            if (sender is TilePanel panel)
            {
                // Find the corresponding ViewModel and update its HeaderColor
                var viewModelEntry = _viewModelToPanelMap.FirstOrDefault(kvp => kvp.Value == panel);
                if (viewModelEntry.Key != null)
                {
                    var viewModel = viewModelEntry.Key;
                    
                    // Temporarily unsubscribe to avoid circular updates
                    viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    
                    // Update ViewModel HeaderColor
                    viewModel.HeaderColor = e.SelectedColor.ToString();
                    
                    // Re-subscribe
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
        }

        private void Panel_TitleChanged(object? sender, string newTitle)
        {
            if (sender is TilePanel panel)
            {
                // Find the corresponding ViewModel and update its Title
                var viewModelEntry = _viewModelToPanelMap.FirstOrDefault(kvp => kvp.Value == panel);
                if (viewModelEntry.Key != null)
                {
                    var viewModel = viewModelEntry.Key;
                    
                    // Temporarily unsubscribe to avoid circular updates
                    viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    
                    // Update ViewModel Title
                    viewModel.Title = newTitle;
                    
                    // Re-subscribe
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
        }

        private void Panel_PanelSelected(object? sender, EventArgs e)
        {
            if (sender is TilePanel panel)
            {
                SelectPanel(panel.PanelId);
            }
        }

        private void UpdateAllPanelsEditMode()
        {
            foreach (var panel in Panels)
            {
                UpdatePanelEditMode(panel);
            }
        }

        private void UpdatePanelEditMode(TilePanel panel)
        {
            panel.IsEditMode = IsEditMode;
        }

        private void UpdateAllPanelsSpacing()
        {
            var spacing = new Thickness(PanelSpacing);
            foreach (var panel in Panels)
            {
                panel.ContentMargin = spacing;
            }
        }

        private void UpdateAllPanelsGap()
        {
            var gap = new Thickness(PanelGap);
            foreach (var panel in Panels)
            {
                panel.PanelMargin = gap;
            }
        }

        /// <summary>
        /// Updates panel selection based on SelectedPaneId property change
        /// </summary>
        private void UpdatePanelSelection(string selectedPaneId)
        {
            UpdateAllViewModelsSelection();
            
            // Raise event if a panel is selected
            if (!string.IsNullOrEmpty(selectedPaneId))
            {
                var panel = Panels.FirstOrDefault(p => p.PanelId == selectedPaneId);
                if (panel != null)
                {
                    var panelLayout = panel.GetLayout();
                    OnPanelSelected(new PanelEventArgs(panelLayout));
                }
            }
        }

        /// <summary>
        /// Sets fallback content when no template is available
        /// </summary>
        private static void SetFallbackContent(TilePanel panel, IPaneViewModel viewModel)
        {
            panel.Content = new TextBlock
            {
                Text = $"Content for {viewModel.Title}\n\nViewModel Type: {viewModel.PaneType}\n\nNo DataTemplate found.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10),
                Foreground = Brushes.DarkGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        /// <summary>
        /// Updates IsSelected property for all ViewModels based on current SelectedPaneId
        /// </summary>
        private void UpdateAllViewModelsSelection()
        {
            foreach (var kvp in _viewModelToPanelMap)
            {
                var viewModel = kvp.Key;
                var panel = kvp.Value;
                
                // Temporarily unsubscribe to avoid circular updates
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                
                // Update IsSelected based on whether this panel is the selected one
                viewModel.IsSelected = panel.PanelId == SelectedPaneId;
                
                // Re-subscribe
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void UpdatePanelPositionsForNewGrid(double[] oldColumnWidths)
        {
            if (GridMode != GridMode.Flexible || oldColumnWidths == null)
                return;

            var newColumnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, GetCanvasWidth());

            foreach (var panel in Panels)
            {
                // Use stored grid coordinates instead of calculating from pixel positions
                var gridColumn = panel.GridColumn;
                var gridColumnSpan = panel.GridColumnSpan;
                var gridRow = panel.GridRow;
                var gridRowSpan = panel.GridRowSpan;
                
                // Calculate new pixel position from grid coordinates
                var newLeft = GridCalculationService.CalculatePositionForColumn(gridColumn, newColumnWidths, PanelGap);
                var newWidth = GridCalculationService.CalculateWidthForColumnSpan(gridColumnSpan, newColumnWidths, PanelGap);
                var newTop = gridRow * Configuration.Grid.GridSize;
                var newHeight = gridRowSpan * Configuration.Grid.GridSize;

                Canvas.SetLeft(panel, newLeft);
                Canvas.SetTop(panel, newTop);
                panel.Width = newWidth;
                panel.Height = newHeight;
                
                // Update corresponding ViewModel if it exists
                var viewModelEntry = _viewModelToPanelMap.FirstOrDefault(kvp => kvp.Value == panel);
                if (viewModelEntry.Key != null)
                {
                    var viewModel = viewModelEntry.Key;
                    viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    viewModel.X = newLeft;
                    viewModel.Y = newTop;
                    viewModel.Width = newWidth;
                    viewModel.Height = newHeight;
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }
            }
        }

        private void DrawGridLines()
        {
            if (_gridLinesCanvas == null) return;

            _gridLinesCanvas.Children.Clear();

            if (!ShowGrid) return;

            var width = Math.Max(GetCanvasWidth(), MinCanvasWidth);
            var height = Math.Max(ActualHeight, MinCanvasHeight);

            if (GridMode == GridMode.Fixed)
            {
                DrawFixedGridLines(width, height);
            }
            else
            {
                DrawFlexibleGridLines(width, height);
            }
        }

        private void DrawFixedGridLines(double width, double height)
        {
            if (_gridLinesCanvas == null) return;
            
            var gridSize = Configuration.Grid.GridSize;
            var stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Configuration.Grid.GridLineColor));

            // Vertical lines
            for (double x = 0; x <= width; x += gridSize)
            {
                var line = new Line
                {
                    X1 = x, Y1 = 0, X2 = x, Y2 = height,
                    Stroke = stroke,
                    StrokeThickness = Configuration.Grid.GridLineThickness
                };
                _gridLinesCanvas.Children.Add(line);
            }

            // Horizontal lines
            for (double y = 0; y <= height; y += gridSize)
            {
                var line = new Line
                {
                    X1 = 0, Y1 = y, X2 = width, Y2 = y,
                    Stroke = stroke,
                    StrokeThickness = Configuration.Grid.GridLineThickness
                };
                _gridLinesCanvas.Children.Add(line);
            }
        }

        private void DrawFlexibleGridLines(double width, double height)
        {
            if (_gridLinesCanvas == null) return;

            var columnWidths = GridCalculationService.CalculateColumnWidths(Configuration.Grid, width);
            var stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Configuration.Grid.GridLineColor));
            var gridSize = Configuration.Grid.GridSize;

            // Vertical column lines
            double currentX = 0;
            for (int i = 0; i <= Configuration.Grid.ColumnCount; i++)
            {
                var isEdge = i == 0 || i == Configuration.Grid.ColumnCount;
                var line = new Line
                {
                    X1 = currentX, Y1 = 0, X2 = currentX, Y2 = height,
                    Stroke = isEdge ? Brushes.Gray : stroke,
                    StrokeThickness = isEdge ? 2 : Configuration.Grid.GridLineThickness
                };
                _gridLinesCanvas.Children.Add(line);

                if (i < Configuration.Grid.ColumnCount)
                    currentX += columnWidths[i];
            }

            // Horizontal lines
            for (double y = 0; y <= height; y += gridSize)
            {
                var line = new Line
                {
                    X1 = 0, Y1 = y, X2 = currentX, Y2 = y,
                    Stroke = stroke,
                    StrokeThickness = Configuration.Grid.GridLineThickness
                };
                _gridLinesCanvas.Children.Add(line);
            }
        }

        #endregion

        #region Event Raising Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void OnPanelAdded(PanelEventArgs e) => PanelAdded?.Invoke(this, e);
        protected virtual void OnPanelRemoved(PanelEventArgs e) => PanelRemoved?.Invoke(this, e);
        protected virtual void OnPanelMoved(PanelEventArgs e) => PanelMoved?.Invoke(this, e);
        protected virtual void OnPanelResized(PanelEventArgs e) => PanelResized?.Invoke(this, e);
        protected virtual void OnPanelSelected(PanelEventArgs e) => PanelSelected?.Invoke(this, e);
        protected virtual void OnLayoutChanged(LayoutChangeType changeType, PanelLayout[] panels) =>
            LayoutChanged?.Invoke(this, new LayoutEventArgs(changeType, panels));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        #endregion
    }
}