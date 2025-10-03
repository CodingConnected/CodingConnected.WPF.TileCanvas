using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CodingConnected.WPF.TileCanvas.Library.ViewModels;
using CodingConnected.WPF.TileCanvas.Example.ViewModels;
using CodingConnected.WPF.TileCanvas.Library.Enums;

namespace CodingConnected.WPF.TileCanvas.Example.Services
{
    /// <summary>
    /// Simplified ViewModel-only serializer that saves complete app state in a single file
    /// </summary>
    public class ViewModelSerializer
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public ViewModelSerializer()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
                // Include all fields to capture private backing fields from ViewModels
                IncludeFields = true
            };
        }

        /// <summary>
        /// Saves complete application state using only ViewModels
        /// </summary>
        /// <param name="panes">Collection of ViewModels</param>
        /// <param name="appSettings">Application settings</param>
        /// <param name="filePath">File path to save to</param>
        public async Task SaveAsync(IEnumerable<IPaneViewModel> panes, AppSettings appSettings, string filePath)
        {
            var saveData = new AppSaveData
            {
                AppSettings = appSettings,
                Panes = [.. panes.Select(ConvertToSerializablePane)],
                SavedAt = DateTime.Now,
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            };

            var json = JsonSerializer.Serialize(saveData, _serializerOptions);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Loads complete application state from ViewModels
        /// </summary>
        /// <param name="filePath">File path to load from</param>
        /// <returns>Tuple with loaded panes and app settings</returns>
        public async Task<(List<IPaneViewModel> Panes, AppSettings AppSettings)> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return (new List<IPaneViewModel>(), new AppSettings());

            var json = await File.ReadAllTextAsync(filePath);
            var saveData = JsonSerializer.Deserialize<AppSaveData>(json, _serializerOptions);

            if (saveData == null)
                return (new List<IPaneViewModel>(), new AppSettings());

            var panes = saveData.Panes.Select(ConvertFromSerializablePane).Where(p => p != null).ToList()!;
            return (panes, saveData.AppSettings ?? new AppSettings());
        }

        /// <summary>
        /// Converts a ViewModel to a serializable format
        /// </summary>
        private SerializablePane ConvertToSerializablePane(IPaneViewModel pane)
        {
            return new SerializablePane
            {
                // Base properties from IPaneViewModel
                Id = pane.Id,
                Title = pane.Title,
                X = pane.X,
                Y = pane.Y,
                Width = pane.Width,
                Height = pane.Height,
                HeaderColor = pane.HeaderColor,
                BackgroundColor = pane.BackgroundColor,
                BorderColor = pane.BorderColor,
                BorderThickness = pane.BorderThickness,
                ShowHeader = pane.ShowHeader,
                CanClose = pane.CanClose,
                IsSelected = pane.IsSelected,
                PaneType = pane.PaneType,

                // Type-specific properties
                TypeSpecificData = SerializeTypeSpecificData(pane)
            };
        }

        /// <summary>
        /// Converts a serializable pane back to a ViewModel
        /// </summary>
        private IPaneViewModel? ConvertFromSerializablePane(SerializablePane serializablePane)
        {
            IPaneViewModel? viewModel = serializablePane.PaneType switch
            {
                "Chart" => new ChartPaneViewModel(serializablePane.Title),
                "Stats" => new StatsPaneViewModel(serializablePane.Title),
                "Table" => new TablePaneViewModel(serializablePane.Title),
                "Label" => new LabelPaneViewModel(serializablePane.Title),
                _ => null
            };

            if (viewModel != null)
            {
                // Restore base properties
                viewModel.X = serializablePane.X;
                viewModel.Y = serializablePane.Y;
                viewModel.Width = serializablePane.Width;
                viewModel.Height = serializablePane.Height;
                viewModel.HeaderColor = serializablePane.HeaderColor;
                viewModel.BackgroundColor = serializablePane.BackgroundColor;
                viewModel.BorderColor = serializablePane.BorderColor;
                viewModel.BorderThickness = serializablePane.BorderThickness;
                viewModel.ShowHeader = serializablePane.ShowHeader;
                viewModel.CanClose = serializablePane.CanClose;
                viewModel.IsSelected = serializablePane.IsSelected;

                // Restore type-specific data
                RestoreTypeSpecificData(viewModel, serializablePane.TypeSpecificData);
            }

            return viewModel;
        }

        /// <summary>
        /// Serializes type-specific data for a ViewModel
        /// </summary>
        private JsonElement? SerializeTypeSpecificData(IPaneViewModel pane)
        {
            object? data = pane switch
            {
                ChartPaneViewModel chart => new
                {
                    chart.ChartType,
                    chart.DataDescription,
                    chart.SeriesInfo,
                    chart.ShowLegend
                },
                StatsPaneViewModel stats => new
                {
                    stats.HeaderText,
                    stats.RefreshInterval,
                    Statistics = stats.Statistics.Select(s => new
                    {
                        s.Label,
                        s.Value,
                        s.Icon,
                        s.TextColor
                    }).ToList()
                },
                TablePaneViewModel table => new
                {
                    table.TableDescription,
                    table.RowCount,
                    table.ColumnCount,
                    table.HasHeader,
                    table.DataSource
                },
                LabelPaneViewModel label => new
                {
                    label.ChartType,
                    label.Title
                },
                _ => null
            };

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, _serializerOptions);
                return JsonSerializer.Deserialize<JsonElement>(json);
            }

            return null;
        }

        /// <summary>
        /// Restores type-specific data for a ViewModel
        /// </summary>
        private void RestoreTypeSpecificData(IPaneViewModel viewModel, JsonElement? typeSpecificData)
        {
            if (!typeSpecificData.HasValue) return;

            var jsonElement = typeSpecificData.Value;

            try
            {
                switch (viewModel)
                {
                    case ChartPaneViewModel chart:
                        if (jsonElement.TryGetProperty("chartType", out var chartType))
                            chart.ChartType = chartType.GetString() ?? "Line";
                        if (jsonElement.TryGetProperty("dataDescription", out var dataDesc))
                            chart.DataDescription = dataDesc.GetString() ?? "Sample chart data";
                        if (jsonElement.TryGetProperty("seriesInfo", out var seriesInfo))
                            chart.SeriesInfo = seriesInfo.GetString() ?? "Chart data";
                        if (jsonElement.TryGetProperty("showLegend", out var showLegend))
                            chart.ShowLegend = showLegend.GetBoolean();
                        break;

                    case StatsPaneViewModel stats:
                        if (jsonElement.TryGetProperty("headerText", out var headerText))
                            stats.HeaderText = headerText.GetString() ?? "Key Metrics";
                        if (jsonElement.TryGetProperty("refreshInterval", out var refreshInterval))
                            stats.RefreshInterval = refreshInterval.GetInt32();

                        // Restore statistics collection
                        if (jsonElement.TryGetProperty("statistics", out var statisticsArray))
                        {
                            stats.Statistics.Clear();
                            foreach (var statElement in statisticsArray.EnumerateArray())
                            {
                                if (statElement.TryGetProperty("label", out var label) &&
                                    statElement.TryGetProperty("value", out var value))
                                {
                                    var icon = statElement.TryGetProperty("icon", out var iconProp) ? iconProp.GetString() ?? "•" : "•";
                                    var textColor = statElement.TryGetProperty("textColor", out var colorProp) ? colorProp.GetString() ?? "#FF000000" : "#FF000000";

                                    stats.Statistics.Add(new StatisticItem(
                                        label.GetString() ?? "",
                                        value.GetString() ?? "",
                                        icon,
                                        textColor
                                    ));
                                }
                            }
                        }
                        break;

                    case TablePaneViewModel table:
                        if (jsonElement.TryGetProperty("tableDescription", out var tableDesc))
                            table.TableDescription = tableDesc.GetString() ?? "Data table";
                        if (jsonElement.TryGetProperty("rowCount", out var rowCount))
                            table.RowCount = rowCount.GetInt32();
                        if (jsonElement.TryGetProperty("columnCount", out var columnCount))
                            table.ColumnCount = columnCount.GetInt32();
                        if (jsonElement.TryGetProperty("hasHeader", out var hasHeader))
                            table.HasHeader = hasHeader.GetBoolean();
                        if (jsonElement.TryGetProperty("dataSource", out var dataSource))
                            table.DataSource = dataSource.GetString() ?? "Sample Dataset";
                        break;

                    case LabelPaneViewModel label:
                        if (jsonElement.TryGetProperty("chartType", out var labelChartType))
                            label.ChartType = labelChartType.GetString() ?? "Label";
                        if (jsonElement.TryGetProperty("title", out var labelTitle))
                            label.Title = labelTitle.GetString() ?? "Label";
                        break;
                }
            }
            catch
            {
                // Ignore deserialization errors for type-specific data
                // The basic properties are already restored
            }
        }
    }

    /// <summary>
    /// Complete application save data structure
    /// </summary>
    public class AppSaveData
    {
        public AppSettings? AppSettings { get; set; }
        public List<SerializablePane> Panes { get; set; } = new();
        public DateTime SavedAt { get; set; }
        public string? AppVersion { get; set; }
    }

    /// <summary>
    /// Application-level settings (replaces the complex AppData structure)
    /// </summary>
    public class AppSettings
    {
        // App state
        public int PanelCounter { get; set; } = 0;
        
        // Canvas configuration
        public GridMode GridMode { get; set; } = GridMode.Flexible;
        public bool ShowGrid { get; set; } = true;
        public bool SnapDrag { get; set; } = true;
        public bool SnapResize { get; set; } = true;
        public int ColumnCount { get; set; } = 7;
        public int MinColumnWidth { get; set; } = 100;
        public bool IsEditMode { get; set; } = true;
        public double PanelSpacing { get; set; } = 5;
        public double PanelGap { get; set; } = 2;
        public double CanvasWidth { get; set; }
    }

    /// <summary>
    /// Serializable representation of a pane (flattened from ViewModel)
    /// </summary>
    public class SerializablePane
    {
        // Base IPaneViewModel properties
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string HeaderColor { get; set; } = "";
        public string BackgroundColor { get; set; } = "";
        public string BorderColor { get; set; } = "";
        public double BorderThickness { get; set; }
        public bool ShowHeader { get; set; }
        public bool CanClose { get; set; }
        public bool IsSelected { get; set; }
        public string PaneType { get; set; } = "";

        // Type-specific data as JSON
        public JsonElement? TypeSpecificData { get; set; }
    }
}