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
    }
}