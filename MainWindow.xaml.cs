using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CodingConnected.WPF.TileCanvas
{
    public partial class MainWindow : Window
    {
        private Point _dragStartPoint;
        private Point _dragStartElementPosition;
        private bool _isDragging;
        private FrameworkElement _draggedElement;
        private int _gridSize = 50;
        private int _panelCounter = 0;
        private const string LayoutFileName = "dashboard-layout.json";

        // Flexible grid properties
        private bool _isFlexibleGrid = false;
        private int _columnCount = 12;
        private double _minColumnWidth = 80;
        private double[] _columnWidths;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize flexible grid
            CalculateColumnWidths();
            DrawGridLines();

            // Add global mouse handlers to the canvas for dragging
            DashboardCanvas.MouseMove += Canvas_MouseMove;
            DashboardCanvas.MouseUp += Canvas_MouseUp;

            // Try to load saved layout, otherwise create default panels
            if (!LoadLayoutFromFile())
            {
                CreateDefaultPanels();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update flexible grid when window size changes
            if (_isFlexibleGrid)
            {
                var oldColumnWidths = _columnWidths?.ToArray(); // Store old widths
                CalculateColumnWidths();

                // Update existing panel widths if column widths changed
                if (oldColumnWidths != null && _columnWidths != null)
                {
                    UpdatePanelWidthsForNewGrid(oldColumnWidths);
                }

                DrawGridLines();
            }
        }

        private void UpdatePanelWidthsForNewGrid(double[] oldColumnWidths)
        {
            if (_columnWidths == null || oldColumnWidths.Length != _columnWidths.Length)
                return;

            foreach (FrameworkElement child in DashboardCanvas.Children)
            {
                if (child is Border panel)
                {
                    var currentLeft = Canvas.GetLeft(panel);
                    if (double.IsNaN(currentLeft)) currentLeft = 0;

                    // Calculate which column this panel starts at and how many columns it spans
                    var startColumn = CalculateStartColumn(currentLeft, oldColumnWidths);
                    var columnSpan = CalculateColumnSpan(panel.Width, oldColumnWidths);

                    // Calculate new position and width based on the new grid
                    var newLeft = CalculatePositionForColumn(startColumn);
                    var newWidth = CalculateWidthForColumnSpan(columnSpan);

                    Canvas.SetLeft(panel, newLeft);
                    panel.Width = newWidth;
                }
            }
        }

        private int CalculateColumnSpan(double panelWidth, double[] columnWidths)
        {
            double totalWidth = 0;
            int span = 1;

            for (int i = 0; i < columnWidths.Length; i++)
            {
                totalWidth += columnWidths[i];
                if (Math.Abs(panelWidth - totalWidth) < 5) // Small tolerance for rounding
                {
                    span = i + 1;
                    break;
                }
            }

            return span;
        }

        private double CalculateWidthForColumnSpan(int columnSpan)
        {
            if (_columnWidths == null || columnSpan <= 0)
                return _minColumnWidth;

            double width = 0;
            for (int i = 0; i < Math.Min(columnSpan, _columnWidths.Length); i++)
            {
                width += _columnWidths[i];
            }

            return width;
        }

        private int CalculateStartColumn(double xPosition, double[] columnWidths)
        {
            double totalWidth = 0;

            for (int i = 0; i < columnWidths.Length; i++)
            {
                if (xPosition <= totalWidth + 5) // Small tolerance for rounding
                {
                    return i;
                }
                totalWidth += columnWidths[i];
            }

            return Math.Max(0, columnWidths.Length - 1); // Last column
        }

        private double CalculatePositionForColumn(int columnIndex)
        {
            if (_columnWidths == null || columnIndex <= 0)
                return 0;

            double position = 0;
            for (int i = 0; i < Math.Min(columnIndex, _columnWidths.Length); i++)
            {
                position += _columnWidths[i];
            }

            return position;
        }

        private void CreateDefaultPanels()
        {
            CreateDashboardPanel("Chart Panel", 0, 0, 400, 300, Colors.LightBlue);
            CreateDashboardPanel("Stats Panel", 400, 0, 300, 300, Colors.LightGreen);
            CreateDashboardPanel("Table Panel", 0, 300, 700, 250, Colors.LightCoral);
        }

        private void CreateDashboardPanel(string title, double x, double y, double width, double height, Color headerColor)
        {
            var border = new Border
            {
                Name = $"Panel_{_panelCounter++}",
                Width = width,
                Height = height,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(2),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(5),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,
                    ShadowDepth = 3,
                    Opacity = 0.3
                }
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            var header = new Border
            {
                Background = new SolidColorBrush(headerColor),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = Cursors.SizeAll,
                Focusable = true
            };

            var headerStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 5, 10, 5),
                Background = Brushes.Transparent // Make it hit-testable but transparent
            };

            var titleText = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false // Prevent this from blocking mouse events
            };

            var closeButton = new Button
            {
                Content = "✕",
                Width = 25,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => DashboardCanvas.Children.Remove(border);

            headerStack.Children.Add(titleText);
            header.Child = headerStack;

            var closeButtonContainer = new Grid();
            closeButtonContainer.Children.Add(closeButton);
            closeButton.HorizontalAlignment = HorizontalAlignment.Right;
            closeButton.Margin = new Thickness(0, 5, 5, 0);

            Grid.SetRow(header, 0);

            // Content
            var content = new Border
            {
                Padding = new Thickness(15),
                Child = new TextBlock
                {
                    Text = $"Content for {title}\n\nDrag to move\nResize from bottom-right corner",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.DarkGray
                }
            };
            Grid.SetRow(content, 1);

            // Resize grip
            var resizeThumb = new Thumb
            {
                Width = 15,
                Height = 15,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNWSE,
                Background = Brushes.DarkGray,
                Margin = new Thickness(0, 0, 3, 3),
                Opacity = 0.5
            };
            resizeThumb.DragDelta += Thumb_DragDelta;
            Grid.SetRowSpan(resizeThumb, 2); // Span across both rows to ensure bottom-right positioning

            grid.Children.Add(header);
            grid.Children.Add(content);
            grid.Children.Add(closeButtonContainer);
            grid.Children.Add(resizeThumb);

            border.Child = grid;

            // Event handlers for dragging - attach to header only
            header.MouseDown += Item_MouseDown;
            header.MouseUp += Item_MouseUp;

            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y);

            DashboardCanvas.Children.Add(border);
        }

        private void Item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Header clicked!"); // Debug output

            if (e.OriginalSource is Button) return; // Don't drag when clicking close

            // Find the border (panel) from the header that was clicked
            var header = sender as Border;
            var grid = header.Parent as Grid;
            var border = grid.Parent as Border;

            _draggedElement = border;
            _dragStartPoint = e.GetPosition(DashboardCanvas);

            // Store the initial element position
            var currentLeft = Canvas.GetLeft(_draggedElement);
            var currentTop = Canvas.GetTop(_draggedElement);
            if (double.IsNaN(currentLeft)) currentLeft = 0;
            if (double.IsNaN(currentTop)) currentTop = 0;
            _dragStartElementPosition = new Point(currentLeft, currentTop);

            _isDragging = true;
            Panel.SetZIndex(_draggedElement, 999);
            e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed && _draggedElement != null)
            {
                var currentPoint = e.GetPosition(DashboardCanvas);
                var totalOffset = currentPoint - _dragStartPoint;

                // Calculate new position from initial position + total offset
                var newLeft = _dragStartElementPosition.X + totalOffset.X;
                var newTop = _dragStartElementPosition.Y + totalOffset.Y;

                // Constrain to canvas bounds before snapping
                newLeft = Math.Max(0, Math.Min(newLeft, DashboardCanvas.ActualWidth - _draggedElement.ActualWidth));
                newTop = Math.Max(0, Math.Min(newTop, DashboardCanvas.ActualHeight - _draggedElement.ActualHeight));

                // Apply appropriate snapping based on grid mode
                if (_isFlexibleGrid)
                {
                    var snappedPos = SnapToFlexibleGrid(newLeft, newTop);
                    Canvas.SetLeft(_draggedElement, snappedPos.X);
                    Canvas.SetTop(_draggedElement, snappedPos.Y);
                }
                else
                {
                    // Fixed grid snapping
                    var snappedLeft = Math.Round(newLeft / _gridSize) * _gridSize;
                    var snappedTop = Math.Round(newTop / _gridSize) * _gridSize;

                    Canvas.SetLeft(_draggedElement, snappedLeft);
                    Canvas.SetTop(_draggedElement, snappedTop);
                }
            }
        }

        private Point SnapToFlexibleGrid(double x, double y)
        {
            if (_columnWidths == null)
            {
                CalculateColumnWidths();
                if (_columnWidths == null) return new Point(x, y);
            }

            // Snap horizontally to column boundaries
            double snappedX = 0;
            double currentX = 0;

            for (int i = 0; i < _columnCount; i++)
            {
                double columnEnd = currentX + _columnWidths[i];

                // Check if x is closer to the start or end of this column
                if (x <= columnEnd)
                {
                    double distToStart = Math.Abs(x - currentX);
                    double distToEnd = Math.Abs(x - columnEnd);

                    snappedX = distToStart <= distToEnd ? currentX : columnEnd;
                    break;
                }

                currentX = columnEnd;
            }

            // Snap vertically to regular grid (same as fixed mode)
            var snappedY = Math.Round(y / _gridSize) * _gridSize;

            return new Point(snappedX, snappedY);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                if (_draggedElement != null)
                {
                    Panel.SetZIndex(_draggedElement, 0);
                }
                _draggedElement = null;
            }
        }

        private void Item_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // This is now handled by Canvas_MouseUp globally
            Canvas_MouseUp(sender, e);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var grid = thumb.Parent as Grid;
            var border = grid.Parent as Border;

            var newWidth = border.Width + e.HorizontalChange;
            var newHeight = border.Height + e.VerticalChange;

            if (_isFlexibleGrid)
            {
                // Snap resize to flexible grid
                var snappedSize = SnapSizeToFlexibleGrid(newWidth, newHeight);
                border.Width = snappedSize.Width;
                border.Height = snappedSize.Height;
            }
            else
            {
                // Snap resize to fixed grid
                var snappedWidth = Math.Round(newWidth / _gridSize) * _gridSize;
                var snappedHeight = Math.Round(newHeight / _gridSize) * _gridSize;

                border.Width = Math.Max(_gridSize * 2, snappedWidth);
                border.Height = Math.Max(_gridSize * 2, snappedHeight);
            }
        }

        private Size SnapSizeToFlexibleGrid(double width, double height)
        {
            if (_columnWidths == null)
            {
                CalculateColumnWidths();
                if (_columnWidths == null) return new Size(width, height);
            }

            // For width, snap to column boundaries (multiple of column widths)
            double snappedWidth = _columnWidths[0]; // Minimum: 1 column
            double totalWidth = 0;

            for (int i = 0; i < _columnCount; i++)
            {
                totalWidth += _columnWidths[i];
                if (width <= totalWidth)
                {
                    snappedWidth = totalWidth;
                    break;
                }
            }

            // For height, use regular grid snapping
            var snappedHeight = Math.Round(height / _gridSize) * _gridSize;
            snappedHeight = Math.Max(_gridSize * 2, snappedHeight);

            return new Size(snappedWidth, snappedHeight);
        }

        private void DrawGridLines()
        {
            if (GridLinesCanvas == null || DashboardCanvas == null) return;

            GridLinesCanvas.Children.Clear();

            if (!ShowGridCheck.IsChecked.Value)
            {
                GridLinesCanvas.Background = Brushes.White;
                return;
            }

            var width = DashboardCanvas.MinWidth;
            var height = DashboardCanvas.MinHeight;

            GridLinesCanvas.MinWidth = width;
            GridLinesCanvas.MinHeight = height;
            DashboardCanvas.MinWidth = width;
            DashboardCanvas.MinHeight = height;

            if (_isFlexibleGrid)
            {
                DrawFlexibleGridLines(width, height);
            }
            else
            {
                DrawFixedGridLines(width, height);
            }
        }

        private void DrawFixedGridLines(double width, double height)
        {
            // Vertical lines
            for (double x = 0; x <= width; x += _gridSize)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                GridLinesCanvas.Children.Add(line);
            }

            // Horizontal lines
            for (double y = 0; y <= height; y += _gridSize)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                GridLinesCanvas.Children.Add(line);
            }
        }

        private void DrawFlexibleGridLines(double width, double height)
        {
            CalculateColumnWidths();

            if (_columnWidths == null) return;

            // Vertical column lines
            double currentX = 0;
            for (int i = 0; i <= _columnCount; i++)
            {
                var line = new Line
                {
                    X1 = currentX,
                    Y1 = 0,
                    X2 = currentX,
                    Y2 = height,
                    Stroke = i == 0 || i == _columnCount ? Brushes.Gray : Brushes.LightGray,
                    StrokeThickness = i == 0 || i == _columnCount ? 2 : 1
                };
                GridLinesCanvas.Children.Add(line);

                if (i < _columnCount)
                    currentX += _columnWidths[i];
            }

            // Horizontal lines every grid size
            for (double y = 0; y <= height; y += _gridSize)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = currentX, // Use actual grid width, not canvas width
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                GridLinesCanvas.Children.Add(line);
            }
        }

        private void GridSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            var selected = (GridSizeCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            _gridSize = selected switch
            {
                "25px" => 25,
                "50px" => 50,
                "100px" => 100,
                _ => 50
            };
            DrawGridLines();
        }

        private void ShowGrid_Changed(object sender, RoutedEventArgs e)
        {
            DrawGridLines();
        }

        private void FlexibleGrid_Changed(object sender, RoutedEventArgs e)
        {
            var wasFlexible = _isFlexibleGrid;
            _isFlexibleGrid = FlexibleGridCheck.IsChecked.Value;

            if (_isFlexibleGrid && !wasFlexible)
            {
                // Switching to flexible mode - adjust existing panels to fit columns
                CalculateColumnWidths();
                AdjustPanelsToFlexibleGrid();
            }

            CalculateColumnWidths();
            DrawGridLines();
        }

        private void AdjustPanelsToFlexibleGrid()
        {
            if (_columnWidths == null) return;

            foreach (FrameworkElement child in DashboardCanvas.Children)
            {
                if (child is Border panel)
                {
                    // Snap existing panel width to nearest column span
                    var bestColumnSpan = FindBestColumnSpanForWidth(panel.Width);
                    var newWidth = CalculateWidthForColumnSpan(bestColumnSpan);
                    panel.Width = newWidth;

                    // Also snap position to column boundaries
                    var currentLeft = Canvas.GetLeft(panel);
                    if (double.IsNaN(currentLeft)) currentLeft = 0;

                    var snappedPos = SnapToFlexibleGrid(currentLeft, Canvas.GetTop(panel));
                    Canvas.SetLeft(panel, snappedPos.X);
                }
            }
        }

        private int FindBestColumnSpanForWidth(double width)
        {
            if (_columnWidths == null) return 1;

            double totalWidth = 0;
            for (int i = 0; i < _columnWidths.Length; i++)
            {
                totalWidth += _columnWidths[i];
                if (width <= totalWidth)
                {
                    return i + 1;
                }
            }

            return _columnWidths.Length; // Full width
        }

        private void ColumnCount_Changed(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(ColumnCountBox.Text, out int count) && count > 0)
            {
                _columnCount = count;
                CalculateColumnWidths();
                DrawGridLines();
            }
        }

        private void MinColumnWidth_Changed(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(MinColumnWidthBox.Text, out double width) && width > 0)
            {
                _minColumnWidth = width;
                CalculateColumnWidths();
                DrawGridLines();
            }
        }

        private void CalculateColumnWidths()
        {
            if (!_isFlexibleGrid || DashboardCanvas == null)
                return;

            var canvasWidth = Math.Max(DashboardCanvas.ActualWidth, DashboardCanvas.MinWidth);
            var totalMinWidth = _columnCount * _minColumnWidth;

            _columnWidths = new double[_columnCount];

            if (totalMinWidth <= canvasWidth)
            {
                // We have extra space, distribute it equally
                var columnWidth = canvasWidth / _columnCount;
                for (int i = 0; i < _columnCount; i++)
                {
                    _columnWidths[i] = columnWidth;
                }
            }
            else
            {
                // Not enough space, use minimum width
                for (int i = 0; i < _columnCount; i++)
                {
                    _columnWidths[i] = _minColumnWidth;
                }
            }
        }

        private void AddPanel_Click(object sender, RoutedEventArgs e)
        {
            var colors = new[] { Colors.LightBlue, Colors.LightGreen, Colors.LightCoral, Colors.LightYellow, Colors.LightPink };
            var random = new Random();
            var color = colors[random.Next(colors.Length)];

            CreateDashboardPanel($"Panel {_panelCounter}", 0, 0, 300, 200, color);
        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            SaveLayoutToFile();
            MessageBox.Show("Layout saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadLayout_Click(object sender, RoutedEventArgs e)
        {
            if (LoadLayoutFromFile())
            {
                MessageBox.Show("Layout loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No saved layout found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetLayout_Click(object sender, RoutedEventArgs e)
        {
            DashboardCanvas.Children.Clear();
            _panelCounter = 0;
            CreateDefaultPanels();
        }

        private void SaveLayoutToFile()
        {
            var layout = new List<PanelLayout>();

            foreach (FrameworkElement child in DashboardCanvas.Children)
            {
                if (child is Border border)
                {
                    var grid = border.Child as Grid;
                    var header = grid.Children[0] as Border;
                    var headerStack = header.Child as StackPanel;
                    var titleText = headerStack.Children[0] as TextBlock;

                    layout.Add(new PanelLayout
                    {
                        Id = border.Name,
                        Title = titleText.Text,
                        X = Canvas.GetLeft(border),
                        Y = Canvas.GetTop(border),
                        Width = border.Width,
                        Height = border.Height,
                        HeaderColor = ((SolidColorBrush)header.Background).Color.ToString()
                    });
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(layout, options);
            File.WriteAllText(LayoutFileName, json);
        }

        private bool LoadLayoutFromFile()
        {
            if (!File.Exists(LayoutFileName))
                return false;

            try
            {
                var json = File.ReadAllText(LayoutFileName);
                var layout = JsonSerializer.Deserialize<List<PanelLayout>>(json);

                DashboardCanvas.Children.Clear();
                _panelCounter = 0;

                foreach (var panel in layout)
                {
                    var color = (Color)ColorConverter.ConvertFromString(panel.HeaderColor);
                    CreateDashboardPanel(panel.Title, panel.X, panel.Y, panel.Width, panel.Height, color);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class PanelLayout
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string HeaderColor { get; set; }
    }
}