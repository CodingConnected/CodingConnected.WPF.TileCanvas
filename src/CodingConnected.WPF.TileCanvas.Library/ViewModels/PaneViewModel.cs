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
        public string Id { get; }

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