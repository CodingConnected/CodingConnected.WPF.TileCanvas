using CodingConnected.WPF.TileCanvas.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodingConnected.WPF.TileCanvas.Library.Services
{
    /// <summary>
    /// Interface for layout serialization services
    /// </summary>
    public interface ILayoutSerializer
    {
        /// <summary>
        /// Serializes a collection of panel layouts to a string
        /// </summary>
        string Serialize(IEnumerable<PanelLayout> panels);

        /// <summary>
        /// Deserializes a string to a collection of panel layouts
        /// </summary>
        IEnumerable<PanelLayout> Deserialize(string data);

        /// <summary>
        /// Saves layout to a file
        /// </summary>
        Task SaveToFileAsync(IEnumerable<PanelLayout> panels, string filePath);

        /// <summary>
        /// Loads layout from a file
        /// </summary>
        Task<IEnumerable<PanelLayout>> LoadFromFileAsync(string filePath);

        /// <summary>
        /// Gets the file extension for this serializer
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Gets the display name for this serializer
        /// </summary>
        string DisplayName { get; }
    }
}