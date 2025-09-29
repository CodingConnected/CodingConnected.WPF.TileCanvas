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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

                // Now snap to grid for final position
                var snappedLeft = Math.Round(newLeft / _gridSize) * _gridSize;
                var snappedTop = Math.Round(newTop / _gridSize) * _gridSize;

                Canvas.SetLeft(_draggedElement, snappedLeft);
                Canvas.SetTop(_draggedElement, snappedTop);
            }
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

            // Snap resize to grid
            var newWidth = Math.Round((border.Width + e.HorizontalChange) / _gridSize) * _gridSize;
            var newHeight = Math.Round((border.Height + e.VerticalChange) / _gridSize) * _gridSize;

            border.Width = Math.Max(_gridSize * 2, newWidth);
            border.Height = Math.Max(_gridSize * 2, newHeight);
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