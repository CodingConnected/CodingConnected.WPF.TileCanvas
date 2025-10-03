using System.ComponentModel;

namespace CodingConnected.WPF.TileCanvas.Library.ViewModels
{
    /// <summary>
    /// Interface for all pane ViewModels that can be displayed in the TileCanvas
    /// </summary>
    public interface IPaneViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Unique identifier for the pane
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display title of the pane
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// X position of the pane on the canvas
        /// </summary>
        double X { get; set; }

        /// <summary>
        /// Y position of the pane on the canvas
        /// </summary>
        double Y { get; set; }

        /// <summary>
        /// Width of the pane
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of the pane
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Header background color (as hex string)
        /// </summary>
        string HeaderColor { get; set; }

        /// <summary>
        /// Pane background color (as hex string)
        /// </summary>
        string BackgroundColor { get; set; }

        /// <summary>
        /// Pane border color (as hex string)
        /// </summary>
        string BorderColor { get; set; }

        /// <summary>
        /// Pane border thickness
        /// </summary>
        double BorderThickness { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the header should be displayed
        /// </summary>
        bool ShowHeader { get; set; }

        /// <summary>
        /// Type identifier for the pane (used for template selection)
        /// </summary>
        string PaneType { get; }

        /// <summary>
        /// Whether the pane can be closed by the user
        /// </summary>
        bool CanClose { get; set; }

        /// <summary>
        /// Whether the pane is currently selected
        /// </summary>
        bool IsSelected { get; set; }
        
        /// <summary>
        /// Starting column index in flexible grid mode (0-based)
        /// </summary>
        int GridColumn { get; set; }
        
        /// <summary>
        /// Number of columns this panel spans in flexible grid mode
        /// </summary>
        int GridColumnSpan { get; set; }
        
        /// <summary>
        /// Starting row index in flexible grid mode (0-based)
        /// </summary>
        int GridRow { get; set; }
        
        /// <summary>
        /// Number of rows this panel spans in flexible grid mode
        /// </summary>
        int GridRowSpan { get; set; }
    }
}