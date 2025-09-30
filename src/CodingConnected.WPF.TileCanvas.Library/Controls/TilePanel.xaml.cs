using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CodingConnected.WPF.TileCanvas.Library.Models;

namespace CodingConnected.WPF.TileCanvas.Library.Controls
{
    /// <summary>
    /// Event args for color changed events
    /// </summary>
    /// <remarks>
    /// Default constructor takes the newly selected color as parameter
    /// </remarks>
    /// <param name="selectedColor">Newly selected color</param>
    public class ColorChangedEventArgs(Color selectedColor) : EventArgs
    {
        /// <summary>
        /// The newly selected color
        /// </summary>
        public Color SelectedColor { get; } = selectedColor;
    }
}

namespace CodingConnected.WPF.TileCanvas.Library.Controls
{
    /// <summary>
    /// Individual draggable and resizable panel control
    /// </summary>
    public partial class TilePanel : UserControl
    {
        #region Private Fields

        private Border? _header;
        private Button? _closeButton;
        private Button? _colorButton;
        private Popup? _colorPopup;
        private UniformGrid? _colorGrid;
        private Thumb? _resizeThumb;
        private TextBlock? _titleDisplay;
        private TextBox? _titleEditor;

        #endregion

        #region Dependency Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(TilePanel),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HeaderBrushProperty =
            DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(TilePanel),
                new PropertyMetadata(new SolidColorBrush(Colors.LightBlue)));

        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register(nameof(IsEditMode), typeof(bool), typeof(TilePanel),
                new PropertyMetadata(true, OnIsEditModeChanged));

        public static readonly DependencyProperty PanelIdProperty =
            DependencyProperty.Register(nameof(PanelId), typeof(string), typeof(TilePanel),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PanelTypeProperty =
            DependencyProperty.Register(nameof(PanelType), typeof(string), typeof(TilePanel),
                new PropertyMetadata(string.Empty));

        // Computed properties for template binding
        public static readonly DependencyPropertyKey HeaderCursorPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HeaderCursor), typeof(Cursor), typeof(TilePanel),
                new PropertyMetadata(Cursors.SizeAll));

        public static readonly DependencyProperty HeaderCursorProperty = HeaderCursorPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey CloseButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CloseButtonVisibility), typeof(Visibility), typeof(TilePanel),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty CloseButtonVisibilityProperty = CloseButtonVisibilityPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey ResizeThumbVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ResizeThumbVisibility), typeof(Visibility), typeof(TilePanel),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ResizeThumbVisibilityProperty = ResizeThumbVisibilityPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ContentMarginProperty =
            DependencyProperty.Register(nameof(ContentMargin), typeof(Thickness), typeof(TilePanel), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty PanelMarginProperty =
            DependencyProperty.Register(nameof(PanelMargin), typeof(Thickness), typeof(TilePanel), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty IsEditingTitleProperty =
            DependencyProperty.Register(nameof(IsEditingTitle), typeof(bool), typeof(TilePanel),
                new PropertyMetadata(false, OnIsEditingTitleChanged));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

        #region Properties

        /// <summary>
        /// Title displayed in the panel header
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Background brush for the panel header
        /// </summary>
        public Brush HeaderBrush
        {
            get => (Brush)GetValue(HeaderBrushProperty);
            set => SetValue(HeaderBrushProperty, value);
        }

        /// <summary>
        /// Whether the panel is in edit mode (can be dragged, resized, closed)
        /// </summary>
        public bool IsEditMode
        {
            get => (bool)GetValue(IsEditModeProperty);
            set => SetValue(IsEditModeProperty, value);
        }

        /// <summary>
        /// Unique identifier for the panel
        /// </summary>
        public string PanelId
        {
            get => (string)GetValue(PanelIdProperty);
            set => SetValue(PanelIdProperty, value);
        }

        /// <summary>
        /// Type identifier for the panel (for custom panel types)
        /// </summary>
        public string PanelType
        {
            get => (string)GetValue(PanelTypeProperty);
            set => SetValue(PanelTypeProperty, value);
        }

        /// <summary>
        /// Cursor for the header area
        /// </summary>
        public Cursor HeaderCursor
        {
            get => (Cursor)GetValue(HeaderCursorProperty);
            private set => SetValue(HeaderCursorPropertyKey, value);
        }

        /// <summary>
        /// Visibility of the close button
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get => (Visibility)GetValue(CloseButtonVisibilityProperty);
            private set => SetValue(CloseButtonVisibilityPropertyKey, value);
        }

        /// <summary>
        /// Visibility of the resize thumb
        /// </summary>
        public Visibility ResizeThumbVisibility
        {
            get => (Visibility)GetValue(ResizeThumbVisibilityProperty);
            private set => SetValue(ResizeThumbVisibilityPropertyKey, value);
        }

        /// <summary>
        /// Margin applied to the content area inside the panel
        /// </summary>
        public Thickness ContentMargin
        {
            get => (Thickness)GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        /// <summary>
        /// Margin applied to the entire panel (spacing between panels)
        /// </summary>
        public Thickness PanelMargin
        {
            get => (Thickness)GetValue(PanelMarginProperty);
            set => SetValue(PanelMarginProperty, value);
        }

        /// <summary>
        /// Whether the title is currently being edited
        /// </summary>
        public bool IsEditingTitle
        {
            get => (bool)GetValue(IsEditingTitleProperty);
            set => SetValue(IsEditingTitleProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when dragging starts
        /// </summary>
        public event EventHandler<MouseButtonEventArgs>? DragStarted;

        /// <summary>
        /// Raised when the panel is resized
        /// </summary>
        public event EventHandler? Resized;

        /// <summary>
        /// Raised when the close button is clicked
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Raised when a color is selected from the color picker
        /// </summary>
        public event EventHandler<ColorChangedEventArgs>? ColorChanged;

        /// <summary>
        /// Raised when the title is changed through inline editing
        /// </summary>
        public event EventHandler<string>? TitleChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TilePanel()
        {
            InitializeComponent();

            // Set default values
            BorderBrush = Brushes.Gray;
            BorderThickness = new Thickness(2);
            Background = Brushes.White;

            // Generate default ID if not set
            if (string.IsNullOrEmpty(PanelId))
            {
                PanelId = Guid.NewGuid().ToString();
            }

            UpdateEditModeVisuals();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a TilePanel from a PanelLayout
        /// </summary>
        public static TilePanel FromLayout(PanelLayout layout)
        {
            var panel = new TilePanel
            {
                PanelId = layout.Id,
                Title = layout.Title,
                Width = layout.Width,
                Height = layout.Height,
                PanelType = layout.PanelType ?? string.Empty
            };

            // Set header color
            if (!string.IsNullOrEmpty(layout.HeaderColor))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(layout.HeaderColor);
                    panel.HeaderBrush = new SolidColorBrush(color);
                }
                catch
                {
                    // Use default if color parsing fails
                }
            }

            // Set content if available
            if (layout.ContentData != null)
            {
                if (layout.ContentData is string text)
                {
                    panel.Content = new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.DarkGray
                    };
                }
                else
                {
                    panel.Content = layout.ContentData;
                }
            }
            else
            {
                // Default content
                panel.Content = new TextBlock
                {
                    Text = $"Content for {layout.Title}\n\nThis is a tile panel.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.DarkGray
                };
            }

            return panel;
        }

        /// <summary>
        /// Gets the current layout information for this panel
        /// </summary>
        public PanelLayout GetLayout()
        {
            var x = Canvas.GetLeft(this);
            var y = Canvas.GetTop(this);

            return new PanelLayout
            {
                Id = PanelId,
                Title = Title,
                X = double.IsNaN(x) ? 0 : x,
                Y = double.IsNaN(y) ? 0 : y,
                Width = ActualWidth > 0 ? ActualWidth : Width,
                Height = ActualHeight > 0 ? ActualHeight : Height,
                HeaderColor = HeaderBrush is SolidColorBrush solidBrush ? solidBrush.Color.ToString() : "#FF87CEEB",
                PanelType = PanelType,
                ContentData = GetContentData()
            };
        }

        #endregion

        #region Template Parts

        /// <summary>
        /// Actions to perform when the control template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Clean up old event handlers
            if (_header != null)
            {
                _header.MouseDown -= Header_MouseDown;
                _header.MouseUp -= Header_MouseUp;
            }

            if (_closeButton != null)
            {
                _closeButton.Click -= CloseButton_Click;
            }

            if (_colorButton != null)
            {
                _colorButton.Click -= ColorButton_Click;
            }

            if (_resizeThumb != null)
            {
                _resizeThumb.DragDelta -= ResizeThumb_DragDelta;
            }

            if (_titleDisplay != null)
            {
                _titleDisplay.MouseDown -= TitleDisplay_MouseDown;
            }

            if (_titleEditor != null)
            {
                _titleEditor.LostFocus -= TitleEditor_LostFocus;
                _titleEditor.KeyDown -= TitleEditor_KeyDown;
            }

            base.OnApplyTemplate();

            // Get template parts
            _header = GetTemplateChild("PART_Header") as Border;
            _closeButton = GetTemplateChild("PART_CloseButton") as Button;
            _colorButton = GetTemplateChild("PART_ColorButton") as Button;
            _colorPopup = GetTemplateChild("PART_ColorPopup") as Popup;
            _colorGrid = GetTemplateChild("PART_ColorGrid") as UniformGrid;
            _resizeThumb = GetTemplateChild("PART_ResizeThumb") as Thumb;
            _titleDisplay = GetTemplateChild("PART_TitleDisplay") as TextBlock;
            _titleEditor = GetTemplateChild("PART_TitleEditor") as TextBox;

            // Set up new event handlers
            if (_header != null)
            {
                _header.MouseDown += Header_MouseDown;
                _header.MouseUp += Header_MouseUp;
            }

            if (_closeButton != null)
            {
                _closeButton.Click += CloseButton_Click;
            }

            if (_colorButton != null)
            {
                _colorButton.Click += ColorButton_Click;
            }

            // Initialize the color palette
            InitializeColorPalette();

            if (_resizeThumb != null)
            {
                _resizeThumb.DragDelta += ResizeThumb_DragDelta;
            }

            // Set up title editing event handlers
            if (_titleDisplay != null)
            {
                _titleDisplay.MouseDown += TitleDisplay_MouseDown;
            }

            if (_titleEditor != null)
            {
                _titleEditor.LostFocus += TitleEditor_LostFocus;
                _titleEditor.KeyDown += TitleEditor_KeyDown;
            }

            UpdateEditModeVisuals();
        }

        #endregion

        #region Event Handlers

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEditMode || e.OriginalSource is Button) return;

            DragStarted?.Invoke(this, e);
        }

        private void Header_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Event is handled by the parent canvas
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_colorPopup != null)
            {
                _colorPopup.IsOpen = true;
            }
        }

        private void ColorSwatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button colorButton && colorButton.Background is SolidColorBrush brush)
            {
                var selectedColor = brush.Color;
                HeaderBrush = new SolidColorBrush(selectedColor);
                
                // Close popup
                if (_colorPopup != null)
                {
                    _colorPopup.IsOpen = false;
                }
                
                // Raise color changed event
                ColorChanged?.Invoke(this, new ColorChangedEventArgs(selectedColor));
            }
        }

        private void InitializeColorPalette()
        {
            if (_colorGrid == null) return;
            
            _colorGrid.Children.Clear();
            
            // Define a set of common colors for the palette
            var colors = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(Color))
                .Select(p => (Color)p.GetValue(null)!)
                .OrderBy(c => {
                    var (r, g, b) = (c.R / 255f, c.G / 255f, c.B / 255f);
                    var (max, min) = (Math.Max(r, Math.Max(g, b)), Math.Min(r, Math.Min(g, b)));
                    var delta = max - min;
                    return delta == 0 ? 0 : (max == r ? 60 * (((g - b) / delta) % 6) : max == g ? 60 * ((b - r) / delta + 2) : 60 * ((r - g) / delta + 4)) + (max == r && g < b ? 360 : 0);
                }) // Sort by hue for better visual grouping
                .ToArray();

            foreach (var color in colors)
            {
                var colorButton = new Button
                {
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(2),
                    Background = new SolidColorBrush(color),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    ToolTip = color.ToString()
                };
                
                colorButton.Click += ColorSwatch_Click;
                _colorGrid.Children.Add(colorButton);
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var newWidth = Width + e.HorizontalChange;
            var newHeight = Height + e.VerticalChange;

            // Find parent TileCanvas for grid snapping
            var parentCanvas = FindParentTileCanvas();
            if (parentCanvas != null && parentCanvas.Configuration.Grid.SnapToGridOnResize)
            {
                // Apply grid snapping when enabled (works for both fixed and flexible modes)
                var snappedSize = parentCanvas.SnapSizeToGrid(new Size(newWidth, newHeight));
                Width = Math.Max(100, snappedSize.Width);
                Height = Math.Max(100, snappedSize.Height);
            }
            else
            {
                // When snapping is disabled, apply direct sizing with minimum constraints
                Width = Math.Max(100, newWidth);
                Height = Math.Max(100, newHeight);
            }

            Resized?.Invoke(this, EventArgs.Empty);
        }

        private void TitleDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Only allow title editing in edit mode and with left mouse button
            if (IsEditMode && e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true; // Prevent drag from starting
                StartTitleEdit();
            }
        }

        private void TitleEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            EndTitleEdit();
        }

        private void TitleEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                EndTitleEdit();
                e.Handled = true;
            }
        }

        #endregion

        #region Private Methods

        private static void OnIsEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TilePanel panel)
            {
                panel.UpdateEditModeVisuals();
            }
        }

        private static void OnIsEditingTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TilePanel panel && (bool)e.NewValue)
            {
                // Focus the title editor when entering edit mode
                panel.Dispatcher.BeginInvoke(new Action(() =>
                {
                    panel._titleEditor?.Focus();
                    panel._titleEditor?.SelectAll();
                }));
            }
        }

        private void UpdateEditModeVisuals()
        {
            HeaderCursor = IsEditMode ? Cursors.SizeAll : Cursors.Arrow;
            CloseButtonVisibility = IsEditMode ? Visibility.Visible : Visibility.Hidden;
            ResizeThumbVisibility = IsEditMode ? Visibility.Visible : Visibility.Hidden;
        }

        private object? GetContentData()
        {
            if (Content is TextBlock textBlock)
            {
                return textBlock.Text;
            }

            return Content;
        }

        private void StartTitleEdit()
        {
            // Set the TextBox text manually (since we removed the binding)
            if (_titleEditor != null)
            {
                _titleEditor.Text = Title;
            }
            
            IsEditingTitle = true;
        }

        private void EndTitleEdit()
        {
            if (IsEditingTitle)
            {
                IsEditingTitle = false;
                
                // Get the updated title from the TextBox and raise the event
                if (_titleEditor != null)
                {
                    var newTitle = _titleEditor.Text?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(newTitle) && newTitle != Title)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TilePanel] Title changed from '{Title}' to '{newTitle}'");
                        Title = newTitle;
                        TitleChanged?.Invoke(this, newTitle);
                    }
                }
            }
        }

        private TileCanvas? FindParentTileCanvas()
        {
            DependencyObject? parent = this;
            while (parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is TileCanvas tileCanvas)
                {
                    return tileCanvas;
                }
            }
            return null;
        }

        #endregion
    }
}