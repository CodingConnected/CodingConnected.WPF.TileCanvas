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

        public async Task SaveToFileAsync(IEnumerable<PanelLayout> panels, string filePath)
        {
            var json = Serialize(panels);
            await File.WriteAllTextAsync(filePath, json);
        }

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