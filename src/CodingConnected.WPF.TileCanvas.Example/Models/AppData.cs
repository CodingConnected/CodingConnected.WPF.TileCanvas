using CodingConnected.WPF.TileCanvas.Library.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CodingConnected.WPF.TileCanvas.Example.Models
{
    /// <summary>
    /// App-specific data that gets stored alongside the library's layout
    /// </summary>
    public class AppData
    {
        /// <summary>
        /// Dictionary of ViewModel-specific data keyed by panel ID
        /// </summary>
        public Dictionary<string, ViewModelData> ViewModelData { get; set; } = new();

        /// <summary>
        /// App-specific canvas settings
        /// </summary>
        public AppCanvasSettings CanvasSettings { get; set; } = new();
    }

    /// <summary>
    /// ViewModel-specific data for a single pane
    /// </summary>
    public class ViewModelData
    {
        /// <summary>
        /// Type of the ViewModel (Chart, Stats, Table)
        /// </summary>
        public string ViewModelType { get; set; } = string.Empty;

        /// <summary>
        /// Serialized ViewModel-specific properties as JSON
        /// </summary>
        public string? Properties { get; set; }
    }

    /// <summary>
    /// App-specific canvas settings (not covered by library)
    /// </summary>
    public class AppCanvasSettings
    {
        /// <summary>
        /// Current panel counter for maintaining unique names
        /// </summary>
        public int PanelCounter { get; set; } = 0;

        /// <summary>
        /// App version that saved this data
        /// </summary>
        public string? AppVersion { get; set; }

        /// <summary>
        /// When this data was saved
        /// </summary>
        public DateTime SavedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Grid mode when layout was saved
        /// </summary>
        public GridMode GridMode { get; set; } = GridMode.Fixed;
        
        /// <summary>
        /// Whether grid was shown when layout was saved
        /// </summary>
        public bool ShowGrid { get; set; } = true;
        
        /// <summary>
        /// Whether snap to grid on drag was enabled when layout was saved
        /// </summary>
        public bool SnapDrag { get; set; } = true;
        
        /// <summary>
        /// Whether snap to grid on resize was enabled when layout was saved
        /// </summary>
        public bool SnapResize { get; set; } = true;
        
        /// <summary>
        /// Column count setting when layout was saved
        /// </summary>
        public int ColumnCount { get; set; } = 7;
        
        /// <summary>
        /// Minimum column width setting when layout was saved
        /// </summary>
        public int MinColumnWidth { get; set; } = 100;
        
        /// <summary>
        /// Edit mode when layout was saved
        /// </summary>
        public bool IsEditMode { get; set; }
        
        /// <summary>
        /// Canvas width when layout was saved (for flexible grid repositioning)
        /// </summary>
        public double? CanvasWidth { get; set; }
    }
}