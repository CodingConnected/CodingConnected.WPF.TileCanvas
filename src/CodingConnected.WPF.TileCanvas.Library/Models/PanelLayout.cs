using CodingConnected.WPF.TileCanvas.Library.Enums;

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
        
        /// <summary>
        /// Grid mode that was active when this layout was saved
        /// </summary>
        public GridMode? GridMode { get; set; }
        
        /// <summary>
        /// Starting column index in flexible grid mode (0-based)
        /// </summary>
        public int? GridColumn { get; set; }
        
        /// <summary>
        /// Number of columns this panel spans in flexible grid mode
        /// </summary>
        public int? GridColumnSpan { get; set; }
        
        /// <summary>
        /// Canvas width when this layout was saved (for flexible grid repositioning)
        /// </summary>
        public double? CanvasWidth { get; set; }
        
        /// <summary>
        /// Column count setting when this layout was saved (for flexible grid repositioning)
        /// </summary>
        public int? ColumnCount { get; set; }
    }
}