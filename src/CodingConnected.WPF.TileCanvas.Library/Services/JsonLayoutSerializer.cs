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

        public string FileExtension => ".json";
        public string DisplayName => "JSON Layout";

        public JsonLayoutSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public string Serialize(IEnumerable<PanelLayout> panels)
        {
            return JsonSerializer.Serialize(panels.ToList(), _options);
        }

        public IEnumerable<PanelLayout> Deserialize(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return Enumerable.Empty<PanelLayout>();

            try
            {
                var panels = JsonSerializer.Deserialize<List<PanelLayout>>(data, _options);
                return panels ?? Enumerable.Empty<PanelLayout>();
            }
            catch (JsonException)
            {
                // Return empty collection if deserialization fails
                return Enumerable.Empty<PanelLayout>();
            }
        }

        public async Task SaveToFileAsync(IEnumerable<PanelLayout> panels, string filePath)
        {
            var json = Serialize(panels);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<IEnumerable<PanelLayout>> LoadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return Enumerable.Empty<PanelLayout>();

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return Deserialize(json);
            }
            catch (IOException)
            {
                // Return empty collection if file read fails
                return Enumerable.Empty<PanelLayout>();
            }
        }
    }
}