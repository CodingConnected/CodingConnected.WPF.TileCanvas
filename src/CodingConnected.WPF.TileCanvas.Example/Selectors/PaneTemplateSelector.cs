using System.Windows;
using System.Windows.Controls;
using CodingConnected.WPF.TileCanvas.Example.ViewModels;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;

namespace CodingConnected.WPF.TileCanvas.Example.Selectors
{
    /// <summary>
    /// DataTemplateSelector that chooses the appropriate DataTemplate based on the ViewModel type
    /// </summary>
    public class PaneTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// DataTemplate for chart panes
        /// </summary>
        public DataTemplate? ChartTemplate { get; set; }

        /// <summary>
        /// DataTemplate for statistics panes
        /// </summary>
        public DataTemplate? StatsTemplate { get; set; }

        /// <summary>
        /// DataTemplate for table panes
        /// </summary>
        public DataTemplate? TableTemplate { get; set; }

        /// <summary>
        /// Default template for unknown or unspecified pane types
        /// </summary>
        public DataTemplate? DefaultTemplate { get; set; }

        /// <summary>
        /// Selects the appropriate DataTemplate based on the item's ViewModel type
        /// </summary>
        /// <param name="item">The ViewModel item</param>
        /// <param name="container">The container element</param>
        /// <returns>The appropriate DataTemplate</returns>
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is IPaneViewModel paneViewModel)
            {
                return paneViewModel.PaneType switch
                {
                    "Chart" => ChartTemplate,
                    "Stats" => StatsTemplate,
                    "Table" => TableTemplate,
                    _ => DefaultTemplate
                };
            }

            // Alternative implementation using type checking if preferred
            return item switch
            {
                ChartPaneViewModel => ChartTemplate,
                StatsPaneViewModel => StatsTemplate,
                TablePaneViewModel => TableTemplate,
                _ => DefaultTemplate
            };
        }
    }
}