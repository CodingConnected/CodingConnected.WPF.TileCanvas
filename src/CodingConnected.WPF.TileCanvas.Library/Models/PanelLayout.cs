namespace CodingConnected.WPF.TileCanvas.Library.Models
{
    /// <summary>
    /// Represents the layout configuration of a single panel
    /// </summary>
    public class PanelLayout
    {
        /// <summary>
        /// Unique identifier for the panel
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display title of the panel
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// X position of the panel on the canvas
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y position of the panel on the canvas
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Width of the panel
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the panel
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Header background color (as string representation)
        /// </summary>
        public string HeaderColor { get; set; } = "#FF87CEEB";

        /// <summary>
        /// Optional content data for the panel
        /// </summary>
        public object? ContentData { get; set; }

        /// <summary>
        /// Panel type identifier for custom panel types
        /// </summary>
        public string? PanelType { get; set; }
    }
}