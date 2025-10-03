using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CodingConnected.WPF.TileCanvas.Library.ViewModels
{
    /// <summary>
    /// Base ViewModel class for all pane types using CommunityToolkit.Mvvm
    /// </summary>
    public abstract partial class PaneViewModel : ObservableObject, IPaneViewModel
    {
        /// <summary>
        /// Unique identifier for the pane
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display title of the pane
        /// </summary>
        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// X position of the pane on the canvas
        /// </summary>
        [ObservableProperty]
        private double _x;

        /// <summary>
        /// Y position of the pane on the canvas
        /// </summary>
        [ObservableProperty]
        private double _y;

        /// <summary>
        /// Width of the pane
        /// </summary>
        [ObservableProperty]
        private double _width = 300;

        /// <summary>
        /// Height of the pane
        /// </summary>
        [ObservableProperty]
        private double _height = 200;

        /// <summary>
        /// Header background color (as hex string)
        /// </summary>
        [ObservableProperty]
        private string _headerColor = "#FF87CEEB"; // LightBlue

        /// <summary>
        /// Pane background color (as hex string)
        /// </summary>
        [ObservableProperty]
        private string _backgroundColor = "#FFFFFFFF"; // White

        /// <summary>
        /// Pane border color (as hex string)
        /// </summary>
        [ObservableProperty]
        private string _borderColor = "#FFBBBBBB"; // Gray

        /// <summary>
        /// Pane border thickness
        /// </summary>
        [ObservableProperty]
        private double _borderThickness = 2.0;

        /// <summary>
        /// Whether the pane header should be shown
        /// </summary>
        [ObservableProperty]
        private bool _showHeader = true;

        /// <summary>
        /// Whether the pane can be closed by the user
        /// </summary>
        [ObservableProperty]
        private bool _canClose = true;

        /// <summary>
        /// Whether the pane is currently selected
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;
        
        /// <summary>
        /// Starting column index in flexible grid mode (0-based)
        /// </summary>
        [ObservableProperty]
        private int _gridColumn;
        
        /// <summary>
        /// Number of columns this panel spans in flexible grid mode
        /// </summary>
        [ObservableProperty]
        private int _gridColumnSpan = 1;
        
        /// <summary>
        /// Starting row index in flexible grid mode (0-based)
        /// </summary>
        [ObservableProperty]
        private int _gridRow;
        
        /// <summary>
        /// Number of rows this panel spans in flexible grid mode
        /// </summary>
        [ObservableProperty]
        private int _gridRowSpan = 1;

        /// <summary>
        /// Type identifier for the pane (used for template selection)
        /// </summary>
        public abstract string PaneType { get; }

        /// <summary>
        /// Protected constructor for base class
        /// </summary>
        /// <param name="title">Initial title for the pane</param>
        protected PaneViewModel(string title)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
        }

        /// <summary>
        /// Constructor with position parameters
        /// </summary>
        /// <param name="title">Initial title for the pane</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        protected PaneViewModel(string title, double x, double y) : this(title)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructor with full parameters
        /// </summary>
        /// <param name="title">Initial title for the pane</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width of the pane</param>
        /// <param name="height">Height of the pane</param>
        protected PaneViewModel(string title, double x, double y, double width, double height) : this(title, x, y)
        {
            Width = width;
            Height = height;
        }
    }
}