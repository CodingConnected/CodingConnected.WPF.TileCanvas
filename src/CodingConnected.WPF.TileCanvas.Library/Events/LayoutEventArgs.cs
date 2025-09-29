using CodingConnected.WPF.TileCanvas.Library.Models;
using System;
using System.Collections.Generic;

namespace CodingConnected.WPF.TileCanvas.Library.Events
{
    /// <summary>
    /// Event arguments for layout change events
    /// </summary>
    public class LayoutEventArgs : EventArgs
    {
        /// <summary>
        /// The type of layout change
        /// </summary>
        public LayoutChangeType ChangeType { get; }

        /// <summary>
        /// List of affected panels
        /// </summary>
        public IReadOnlyList<PanelLayout> AffectedPanels { get; }

        /// <summary>
        /// Optional data related to the layout change
        /// </summary>
        public object? Data { get; }

        public LayoutEventArgs(LayoutChangeType changeType, IReadOnlyList<PanelLayout> affectedPanels, object? data = null)
        {
            ChangeType = changeType;
            AffectedPanels = affectedPanels;
            Data = data;
        }

        public LayoutEventArgs(LayoutChangeType changeType, PanelLayout panel, object? data = null)
            : this(changeType, new[] { panel }, data)
        {
        }
    }

    /// <summary>
    /// Types of layout changes
    /// </summary>
    public enum LayoutChangeType
    {
        PanelAdded,
        PanelRemoved,
        PanelMoved,
        PanelResized,
        GridConfigurationChanged,
        EditModeChanged,
        LayoutLoaded,
        LayoutCleared
    }
}