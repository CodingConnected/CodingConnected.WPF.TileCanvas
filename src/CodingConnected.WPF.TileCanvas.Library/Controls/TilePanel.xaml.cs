using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CodingConnected.WPF.TileCanvas.Library.Models;

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
        private Thumb? _resizeThumb;

        #endregion

        #region Dependency Properties

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

        #endregion

        #region Constructor

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

            if (_resizeThumb != null)
            {
                _resizeThumb.DragDelta -= ResizeThumb_DragDelta;
            }

            base.OnApplyTemplate();

            // Get template parts
            _header = GetTemplateChild("PART_Header") as Border;
            _closeButton = GetTemplateChild("PART_CloseButton") as Button;
            _resizeThumb = GetTemplateChild("PART_ResizeThumb") as Thumb;

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

            if (_resizeThumb != null)
            {
                _resizeThumb.DragDelta += ResizeThumb_DragDelta;
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

        #endregion

        #region Private Methods

        private static void OnIsEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TilePanel panel)
            {
                panel.UpdateEditModeVisuals();
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