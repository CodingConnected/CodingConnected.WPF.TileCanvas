using CommunityToolkit.Mvvm.ComponentModel;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;

namespace CodingConnected.WPF.TileCanvas.Example.ViewModels
{
    /// <summary>
    /// ViewModel for data table panes
    /// </summary>
    public partial class TablePaneViewModel : PaneViewModel
    {
        /// <summary>
        /// Description of the table content
        /// </summary>
        [ObservableProperty]
        private string _tableDescription = "📋 Data Table\n\nThis panel could contain a data grid or table.\n\nResize me using the handle in the bottom-right corner!";

        /// <summary>
        /// Number of rows in the table (for display purposes)
        /// </summary>
        [ObservableProperty]
        private int _rowCount = 25;

        /// <summary>
        /// Number of columns in the table (for display purposes)
        /// </summary>
        [ObservableProperty]
        private int _columnCount = 5;

        /// <summary>
        /// Whether the table has a header row
        /// </summary>
        [ObservableProperty]
        private bool _hasHeader = true;

        /// <summary>
        /// Table data source name or description
        /// </summary>
        [ObservableProperty]
        private string _dataSource = "Sample Dataset";

        /// <inheritdoc />
        public override string PaneType => "Table";

        /// <summary>
        /// Default constructor
        /// </summary>
        public TablePaneViewModel() : base("Data Table")
        {
            Width = 700;
            Height = 250;
            HeaderColor = "#FFF08080"; // LightCoral
        }

        /// <summary>
        /// Constructor with title
        /// </summary>
        /// <param name="title">Table title</param>
        public TablePaneViewModel(string title) : base(title)
        {
            Width = 700;
            Height = 250;
            HeaderColor = "#FFF08080"; // LightCoral
        }

        /// <summary>
        /// Constructor with position
        /// </summary>
        /// <param name="title">Table title</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public TablePaneViewModel(string title, double x, double y) : base(title, x, y)
        {
            Width = 700;
            Height = 250;
            HeaderColor = "#FFF08080"; // LightCoral
        }

        /// <summary>
        /// Constructor with size
        /// </summary>
        /// <param name="title">Table title</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width of the table pane</param>
        /// <param name="height">Height of the table pane</param>
        public TablePaneViewModel(string title, double x, double y, double width, double height) 
            : base(title, x, y, width, height)
        {
            HeaderColor = "#FFF08080"; // LightCoral
        }
    }
}