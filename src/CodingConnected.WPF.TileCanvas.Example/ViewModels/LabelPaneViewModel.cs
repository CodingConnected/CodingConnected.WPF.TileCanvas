using CommunityToolkit.Mvvm.ComponentModel;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;

namespace CodingConnected.WPF.TileCanvas.Example.ViewModels
{
    /// <summary>
    /// ViewModel for chart-based panes
    /// </summary>
    public partial class LabelPaneViewModel : PaneViewModel
    {
        /// <summary>
        /// Type of chart to display
        /// </summary>
        [ObservableProperty]
        private string _chartType = "Label";

        /// <summary>
        /// Chart data description
        /// </summary>
        [ObservableProperty]
        private string _title = "Title";

        /// <inheritdoc />
        public override string PaneType => "Label";

        /// <summary>
        /// Default constructor
        /// </summary>
        public LabelPaneViewModel() : base("Title Panel")
        {
            Width = 400;
            Height = 300;
            HeaderColor = "#FF87CEEB"; // LightBlue
        }

        /// <summary>
        /// Constructor with title
        /// </summary>
        /// <param name="title">Chart title</param>
        public LabelPaneViewModel(string title) : base(title)
        {
            _title = title;
            Width = 400;
            Height = 300;
            HeaderColor = "#FF87CEEB"; // LightBlue
        }

        /// <summary>
        /// Constructor with position
        /// </summary>
        /// <param name="title">Chart title</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public LabelPaneViewModel(string title, double x, double y) : base(title, x, y)
        {
            _title = title;
            Width = 400;
            Height = 300;
            HeaderColor = "#FF87CEEB"; // LightBlue
        }
    }
}