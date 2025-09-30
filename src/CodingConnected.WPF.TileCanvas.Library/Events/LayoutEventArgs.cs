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

        /// <summary>
        /// Constructor for LayoutEventArgs with layout change details
        /// </summary>
        /// <param name="changeType">The type of layout change</param>
        /// <param name="affectedPanels">List of affected panels</param>
        /// <param name="data">Optional data related to the layout change</param>
        public LayoutEventArgs(LayoutChangeType changeType, IReadOnlyList<PanelLayout> affectedPanels, object? data = null)
        {
            ChangeType = changeType;
            AffectedPanels = affectedPanels;
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutEventArgs"/> class with the specified layout change type,
        /// panel, and optional data.
        /// </summary>
        /// <param name="changeType">The type of layout change that occurred.</param>
        /// <param name="panel">The panel associated with the layout change.</param>
        /// <param name="data">Optional additional data related to the layout change. This parameter can be <see langword="null"/>.</param>
        public LayoutEventArgs(LayoutChangeType changeType, PanelLayout panel, object? data = null)
            : this(changeType, [panel], data)
        {
        }
    }

    /// <summary>
    /// Types of layout changes
    /// </summary>
    public enum LayoutChangeType
    {
        /// <summary>
        /// Panel added
        /// </summary>
        PanelAdded,
        /// <summary>
        /// Panel removed
        /// </summary>
        PanelRemoved,
        /// <summary>
        /// Panel moved
        /// </summary>
        PanelMoved,
        /// <summary>
        /// Panel resized
        /// </summary>
        PanelResized,
        /// <summary>
        /// Grid configuration changed (e.g., grid mode switched)
        /// </summary>
        GridConfigurationChanged,
        /// <summary>
        /// Edit mode toggled
        /// </summary>
        EditModeChanged,
        /// <summary>
        /// Layout loaded from external source
        /// </summary>
        LayoutLoaded,
        /// <summary>
        /// Layout cleared (all panels removed)
        /// </summary>
        LayoutCleared
    }
}