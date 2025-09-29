using CommunityToolkit.Mvvm.ComponentModel;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodingConnected.WPF.TileCanvas.Example.ViewModels
{
    /// <summary>
    /// Represents a single statistic item
    /// </summary>
    public partial class StatisticItem : ObservableObject
    {
        [ObservableProperty]
        private string _label = string.Empty;

        [ObservableProperty]
        private string _value = string.Empty;

        [ObservableProperty]
        private string _icon = string.Empty;

        [ObservableProperty]
        private string _textColor = "#FF000000"; // Black

        public StatisticItem(string label, string value, string icon = "", string textColor = "#FF000000")
        {
            Label = label;
            Value = value;
            Icon = icon;
            TextColor = textColor;
        }
    }

    /// <summary>
    /// ViewModel for statistics/metrics panes
    /// </summary>
    public partial class StatsPaneViewModel : PaneViewModel
    {
        /// <summary>
        /// Collection of statistics to display
        /// </summary>
        public ObservableCollection<StatisticItem> Statistics { get; }

        /// <summary>
        /// Main header text for the statistics panel
        /// </summary>
        [ObservableProperty]
        private string _headerText = "📈 Key Metrics";

        /// <summary>
        /// Refresh interval in seconds (for future use)
        /// </summary>
        [ObservableProperty]
        private int _refreshInterval = 30;

        /// <inheritdoc />
        public override string PaneType => "Stats";

        /// <summary>
        /// Default constructor
        /// </summary>
        public StatsPaneViewModel() : base("Statistics")
        {
            Width = 300;
            Height = 300;
            HeaderColor = "#FF90EE90"; // LightGreen
            Statistics = new ObservableCollection<StatisticItem>();
            
            // Initialize with default statistics
            InitializeDefaultStats();
        }

        /// <summary>
        /// Constructor with title
        /// </summary>
        /// <param name="title">Statistics panel title</param>
        public StatsPaneViewModel(string title) : base(title)
        {
            Width = 300;
            Height = 300;
            HeaderColor = "#FF90EE90"; // LightGreen
            Statistics = new ObservableCollection<StatisticItem>();
            
            InitializeDefaultStats();
        }

        /// <summary>
        /// Constructor with position
        /// </summary>
        /// <param name="title">Statistics panel title</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public StatsPaneViewModel(string title, double x, double y) : base(title, x, y)
        {
            Width = 300;
            Height = 300;
            HeaderColor = "#FF90EE90"; // LightGreen
            Statistics = new ObservableCollection<StatisticItem>();
            
            InitializeDefaultStats();
        }

        /// <summary>
        /// Initialize default statistics data
        /// </summary>
        private void InitializeDefaultStats()
        {
            Statistics.Add(new StatisticItem("Total Users", "1,234", "•"));
            Statistics.Add(new StatisticItem("Active Sessions", "89", "•"));
            Statistics.Add(new StatisticItem("Revenue", "$45,678", "•"));
            Statistics.Add(new StatisticItem("Growth", "+12.5%", "•", "#FF008000")); // Green
        }

        /// <summary>
        /// Updates a statistic value by label
        /// </summary>
        /// <param name="label">The statistic label to update</param>
        /// <param name="newValue">The new value</param>
        public void UpdateStatistic(string label, string newValue)
        {
            var stat = Statistics.FirstOrDefault(s => s.Label == label);
            if (stat != null)
            {
                stat.Value = newValue;
            }
        }

        /// <summary>
        /// Adds a new statistic to the collection
        /// </summary>
        /// <param name="label">Statistic label</param>
        /// <param name="value">Statistic value</param>
        /// <param name="icon">Optional icon</param>
        /// <param name="textColor">Text color (hex string)</param>
        public void AddStatistic(string label, string value, string icon = "•", string textColor = "#FF000000")
        {
            Statistics.Add(new StatisticItem(label, value, icon, textColor));
        }
    }
}