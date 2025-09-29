using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using CodingConnected.WPF.TileCanvas.Library.Controls;
using CodingConnected.WPF.TileCanvas.Library.Enums;
using CodingConnected.WPF.TileCanvas.Library.Events;

namespace CodingConnected.WPF.TileCanvas.Example
{
    /// <summary>
    /// Example application demonstrating the Tile Canvas library
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _panelCounter = 0;
        private readonly Color[] _panelColors = {
            Colors.LightBlue, Colors.LightGreen, Colors.LightCoral,
            Colors.LightYellow, Colors.LightPink, Colors.LightCyan,
            Colors.LightGray, Colors.LightGoldenrodYellow
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize UI state
            ColumnCountBox.IsEnabled = false; // Start in Fixed mode

            // Create some default panels to demonstrate the library
            CreateDefaultPanels();
        }

        #region Event Handlers

        private void AddPanel_Click(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            var color = _panelColors[random.Next(_panelColors.Length)];

            var panel = new TilePanel
            {
                Title = $"Panel {++_panelCounter}",
                Width = 300,
                Height = 200,
                HeaderBrush = new SolidColorBrush(color),
                Content = new TextBlock
                {
                    Text = $"This is panel {_panelCounter}\n\nClick and drag the header to move.\nUse the resize handle in the bottom-right.",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10),
                    Foreground = Brushes.DarkGray
                }
            };

            // Add at a random position
            var x = random.Next(0, 500);
            var y = random.Next(0, 300);

            TileCanvas.AddPanel(panel, x, y);
        }

        private async void SaveLayout_Click(object sender, RoutedEventArgs e)
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
                    await TileCanvas.SaveLayoutAsync(saveDialog.FileName);
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

        private async void LoadLayout_Click(object sender, RoutedEventArgs e)
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
                    await TileCanvas.LoadLayoutAsync(openDialog.FileName);
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

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear all panels?",
                "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                TileCanvas.ClearPanels();
                _panelCounter = 0;
            }
        }

        private void GridMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (TileCanvas == null) return;

            var selectedItem = GridModeCombo.SelectedItem as ComboBoxItem;
            var mode = selectedItem?.Content.ToString() == "Flexible" ? GridMode.Flexible : GridMode.Fixed;

            TileCanvas.GridMode = mode;

            // Update configuration for flexible mode
            if (mode == GridMode.Flexible)
            {
                TileCanvas.Configuration.Grid.ColumnCount = int.Parse(ColumnCountBox.Text);
                TileCanvas.Configuration.Grid.MinColumnWidth = 80;
                ColumnCountBox.IsEnabled = true;
            }
            else
            {
                TileCanvas.Configuration.Grid.GridSize = 50;
                ColumnCountBox.IsEnabled = false;
            }

            TileCanvas.RefreshGrid();
        }

        private void ColumnCount_Changed(object sender, TextChangedEventArgs e)
        {
            if (TileCanvas == null) return;

            if (int.TryParse(ColumnCountBox.Text, out int count) && count > 0 && count <= 24)
            {
                TileCanvas.Configuration.Grid.ColumnCount = count;
                TileCanvas.RefreshGrid();
            }
        }

        private void ShowGrid_Changed(object sender, RoutedEventArgs e)
        {
            if (TileCanvas != null)
            {
                TileCanvas.ShowGrid = ShowGridCheck.IsChecked == true;
            }
        }

        private void EditMode_Changed(object sender, RoutedEventArgs e)
        {
            if (TileCanvas != null)
            {
                TileCanvas.EditMode = EditModeCheck.IsChecked == true ? EditMode.Edit : EditMode.View;
            }
        }

        private void SnapDrag_Changed(object sender, RoutedEventArgs e)
        {
            if (TileCanvas != null)
            {
                TileCanvas.SnapToGridOnDrag = SnapDragCheck.IsChecked == true;
            }
        }

        private void SnapResize_Changed(object sender, RoutedEventArgs e)
        {
            if (TileCanvas != null)
            {
                TileCanvas.SnapToGridOnResize = SnapResizeCheck.IsChecked == true;
            }
        }

        // Tile Canvas Event Handlers
        private void TileCanvas_PanelAdded(object? sender, PanelEventArgs e)
        {
            // You can handle panel addition here
            // For example, logging or updating UI state
        }

        private void TileCanvas_PanelRemoved(object? sender, PanelEventArgs e)
        {
            // You can handle panel removal here
        }

        private void TileCanvas_LayoutChanged(object? sender, LayoutEventArgs e)
        {
            // You can handle layout changes here
            // For example, updating a status bar or enabling/disabling buttons
        }

        #endregion

        #region Private Methods

        private void CreateDefaultPanels()
        {
            // Chart Panel
            var chartPanel = new TilePanel
            {
                Title = "Chart Panel",
                Width = 400,
                Height = 300,
                HeaderBrush = new SolidColorBrush(Colors.LightBlue),
                Content = new TextBlock
                {
                    Text = "📊 Chart Panel\n\nThis would contain charts and graphs.\n\nTry dragging me around!",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10),
                    Foreground = Brushes.DarkGray,
                    FontSize = 14
                }
            };
            TileCanvas.AddPanel(chartPanel, 0, 0);

            // Stats Panel
            var statsPanel = new TilePanel
            {
                Title = "Statistics",
                Width = 300,
                Height = 300,
                HeaderBrush = new SolidColorBrush(Colors.LightGreen),
                Content = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
                    {
                        new TextBlock { Text = "📈 Key Metrics", FontWeight = FontWeights.Bold, FontSize = 16, Margin = new Thickness(0, 0, 0, 10) },
                        new TextBlock { Text = "• Total Users: 1,234", Margin = new Thickness(2) },
                        new TextBlock { Text = "• Active Sessions: 89", Margin = new Thickness(2) },
                        new TextBlock { Text = "• Revenue: $45,678", Margin = new Thickness(2) },
                        new TextBlock { Text = "• Growth: +12.5%", Margin = new Thickness(2), Foreground = Brushes.Green }
                    }
                }
            };
            TileCanvas.AddPanel(statsPanel, 400, 0);

            // Table Panel
            var tablePanel = new TilePanel
            {
                Title = "Data Table",
                Width = 700,
                Height = 250,
                HeaderBrush = new SolidColorBrush(Colors.LightCoral),
                Content = new TextBlock
                {
                    Text = "📋 Data Table\n\nThis panel could contain a data grid or table.\n\nResize me using the handle in the bottom-right corner!",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10),
                    Foreground = Brushes.DarkGray,
                    FontSize = 14
                }
            };
            TileCanvas.AddPanel(tablePanel, 0, 300);

            _panelCounter = 3;
        }

        #endregion
    }
}