using CodingConnected.WPF.TileCanvas.Library.Enums;

namespace CodingConnected.WPF.TileCanvas.Library.Models
{
    /// <summary>
    /// Configuration settings for the grid system
    /// </summary>
    public class GridConfiguration
    {
        /// <summary>
        /// Grid layout mode (Fixed or Flexible)
        /// </summary>
        public GridMode Mode { get; set; } = GridMode.Fixed;

        /// <summary>
        /// Size of grid cells in fixed mode (in pixels)
        /// </summary>
        public int GridSize { get; set; } = 50;

        /// <summary>
        /// Number of columns in flexible mode
        /// </summary>
        public int ColumnCount { get; set; } = 7;

        /// <summary>
        /// Minimum column width in pixels for flexible grid mode
        /// </summary>
        public int MinColumnWidth { get; set; } = 100;

        /// <summary>
        /// Whether to show grid lines
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Grid line color
        /// </summary>
        public string GridLineColor { get; set; } = "#FFD3D3D3";

        /// <summary>
        /// Grid line thickness
        /// </summary>
        public double GridLineThickness { get; set; } = 1.0;

        /// <summary>
        /// Whether to snap panels to grid when dragging
        /// </summary>
        public bool SnapToGridOnDrag { get; set; } = true;

        /// <summary>
        /// Whether to snap panels to grid when resizing
        /// </summary>
        public bool SnapToGridOnResize { get; set; } = true;
    }
}