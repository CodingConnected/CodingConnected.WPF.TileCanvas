using CodingConnected.WPF.TileCanvas.Library.Models;
using System;

namespace CodingConnected.WPF.TileCanvas.Library.Events
{
    /// <summary>
    /// Event arguments for panel-related events
    /// </summary>
    public class PanelEventArgs : EventArgs
    {
        /// <summary>
        /// The panel that triggered the event
        /// </summary>
        public PanelLayout Panel { get; }

        /// <summary>
        /// Optional additional data related to the event
        /// </summary>
        public object? Data { get; }

        public PanelEventArgs(PanelLayout panel, object? data = null)
        {
            Panel = panel;
            Data = data;
        }
    }
}