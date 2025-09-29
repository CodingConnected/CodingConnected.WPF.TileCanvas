using CommunityToolkit.Mvvm.ComponentModel;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;

namespace CodingConnected.WPF.TileCanvas.Example.ViewModels
{
    /// <summary>
    /// ViewModel for chart-based panes
    /// </summary>
    public partial class ChartPaneViewModel : PaneViewModel
    {
        /// <summary>
        /// Type of chart to display
        /// </summary>
        [ObservableProperty]
        private string _chartType = "Line";

        /// <summary>
        /// Chart data description
        /// </summary>
        [ObservableProperty]
        private string _dataDescription = "Sample chart data";

        /// <summary>
        /// Chart series information
        /// </summary>
        [ObservableProperty]
        private string _seriesInfo = "📊 Chart Panel\n\nThis would contain charts and graphs.\n\nTry dragging me around!";

        /// <summary>
        /// Whether to show chart legend
        /// </summary>
        [ObservableProperty]
        private bool _showLegend = true;

        /// <inheritdoc />
        public override string PaneType => "Chart";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChartPaneViewModel() : base("Chart Panel")
        {
            Width = 400;
            Height = 300;
            HeaderColor = "#FF87CEEB"; // LightBlue
        }

        /// <summary>
        /// Constructor with title
        /// </summary>
        /// <param name="title">Chart title</param>
        public ChartPaneViewModel(string title) : base(title)
        {
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
        public ChartPaneViewModel(string title, double x, double y) : base(title, x, y)
        {
            Width = 400;
            Height = 300;
            HeaderColor = "#FF87CEEB"; // LightBlue
        }
    }
}