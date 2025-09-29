using CodingConnected.WPF.TileCanvas.Library.Enums;

namespace CodingConnected.WPF.TileCanvas.Library.Models
{
    /// <summary>
    /// Configuration settings for the tile canvas
    /// </summary>
    public class CanvasConfiguration
    {
        /// <summary>
        /// Current edit mode
        /// </summary>
        public bool IsEditMode { get; set; }

        /// <summary>
        /// Grid configuration
        /// </summary>
        public GridConfiguration Grid { get; set; } = new GridConfiguration();

        /// <summary>
        /// Minimum canvas width
        /// </summary>
        public double MinWidth { get; set; } = 1000;

        /// <summary>
        /// Minimum canvas height
        /// </summary>
        public double MinHeight { get; set; } = 800;

        /// <summary>
        /// Canvas background color
        /// </summary>
        public string BackgroundColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        /// Whether to enable panel animations
        /// </summary>
        public bool EnableAnimations { get; set; } = true;

        /// <summary>
        /// Animation duration in milliseconds
        /// </summary>
        public int AnimationDuration { get; set; } = 200;
    }
}