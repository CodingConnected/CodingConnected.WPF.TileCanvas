using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CodingConnected.WPF.TileCanvas.Library.Models;

namespace CodingConnected.WPF.TileCanvas.Library.Services
{
    /// <summary>
    /// JSON implementation of the layout serializer
    /// </summary>
    public class JsonLayoutSerializer : ILayoutSerializer
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// File extension used for JSON layout files
        /// </summary>
        public string FileExtension => ".json";

        /// <summary>
        /// Display name for the JSON layout serializer
        /// </summary>
        public string DisplayName => "JSON Layout";

        /// <summary>
        /// Default constructor initializing JSON serializer options
        /// </summary>
        public JsonLayoutSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Serializes a collection of PanelLayout objects to a JSON string
        /// </summary>
        /// <param name="panels">The panels to serialize</param>
        /// <returns>JSON string with serialization data</returns>
        public string Serialize(IEnumerable<PanelLayout> panels)
        {
            return JsonSerializer.Serialize(panels.ToList(), _options);
        }

        /// <summary>
        /// Deserializes a JSON string to a collection of PanelLayout objects
        /// </summary>
        /// <param name="data">JSON string</param>
        /// <returns>Collection of PanelLayout objects</returns>
        public IEnumerable<PanelLayout> Deserialize(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return [];

            try
            {
                var panels = JsonSerializer.Deserialize<List<PanelLayout>>(data, _options);
                return panels ?? [];
            }
            catch (JsonException)
            {
                // Return empty collection if deserialization fails
                return [];
            }
        }

        /// <summary>
        /// Saves the serialized panel layouts to a file asynchronously
        /// </summary>
        /// <param name="panels">Collection of panels</param>
        /// <param name="filePath">File to save to</param>
        /// <returns>Task that can be awaited</returns>
        public async Task SaveToFileAsync(IEnumerable<PanelLayout> panels, string filePath)
        {
            var json = Serialize(panels);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Loads and deserializes panel layouts from a file asynchronously
        /// </summary>
        /// <param name="filePath">File to load data from</param>
        /// <returns>Awaitable task returning a list of panels</returns>
        public async Task<IEnumerable<PanelLayout>> LoadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return [];

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return Deserialize(json);
            }
            catch (IOException)
            {
                // Return empty collection if file read fails
                return [];
            }
        }
    }
}