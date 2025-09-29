using System;
using CodingConnected.WPF.TileCanvas.Library.Services;
using CodingConnected.WPF.TileCanvas.Library.Models;
using CodingConnected.WPF.TileCanvas.Library.Enums;

class GridPositionTest 
{
    static void Main()
    {
        var gridService = new GridCalculationService();
        var gridConfig = new GridConfiguration
        {
            Mode = GridMode.Flexible,
            ColumnCount = 7,
            MinColumnWidth = 120
        };

        Console.WriteLine("=== Flexible Grid Positioning Test ===\n");

        // Test scenario: Original 1200px canvas, 7 columns
        double originalWidth = 1200.0;
        var originalColumnWidths = gridService.CalculateColumnWidths(gridConfig, originalWidth);
        
        Console.WriteLine($"Original Canvas: {originalWidth}px, {gridConfig.ColumnCount} columns");
        Console.WriteLine($"Column widths: [{string.Join(", ", Array.ConvertAll(originalColumnWidths, x => x.ToString("F1")))}]");
        Console.WriteLine($"Total width: {originalColumnWidths.Sum():F1}px\n");

        // Test panels at different positions
        var testPanels = new[]
        {
            new { Name = "Chart", X = 0.0, Width = 343.0 },      // Columns 0-1
            new { Name = "Stats", X = 343.0, Width = 171.0 },    // Column 2  
            new { Name = "Table", X = 514.0, Width = 686.0 }     // Columns 3-6
        };

        Console.WriteLine("Original Panel Positions:");
        foreach (var panel in testPanels)
        {
            var startCol = gridService.CalculateStartColumn(panel.X, originalColumnWidths);
            var span = gridService.CalculateColumnSpan(panel.Width, originalColumnWidths);
            Console.WriteLine($"  {panel.Name}: X={panel.X:F0}, W={panel.Width:F0} -> Column {startCol}, Span {span}");
        }

        Console.WriteLine("\n=== Simulating Canvas Resize to 1000px ===\n");

        // Test scenario: Resized to 1000px canvas
        double newWidth = 1000.0;
        var newColumnWidths = gridService.CalculateColumnWidths(gridConfig, newWidth);
        
        Console.WriteLine($"New Canvas: {newWidth}px, {gridConfig.ColumnCount} columns");
        Console.WriteLine($"Column widths: [{string.Join(", ", Array.ConvertAll(newColumnWidths, x => x.ToString("F1")))}]");
        Console.WriteLine($"Total width: {newColumnWidths.Sum():F1}px\n");

        Console.WriteLine("Repositioned Panel Positions:");
        foreach (var panel in testPanels)
        {
            // Calculate original grid position
            var startCol = gridService.CalculateStartColumn(panel.X, originalColumnWidths);
            var span = gridService.CalculateColumnSpan(panel.Width, originalColumnWidths);
            
            // Calculate new position based on grid
            var newX = gridService.CalculatePositionForColumn(startCol, newColumnWidths);
            var newWidthCalc = gridService.CalculateWidthForColumnSpan(span, newColumnWidths);
            
            Console.WriteLine($"  {panel.Name}: {panel.X:F0}px -> {newX:F0}px, {panel.Width:F0}px -> {newWidthCalc:F0}px [Col {startCol}, Span {span}]");
        }

        Console.WriteLine("\n=== Testing Column Count Change: 7->5 columns ===\n");

        // Test scenario: Different column count
        var newGridConfig = new GridConfiguration
        {
            Mode = GridMode.Flexible,
            ColumnCount = 5,
            MinColumnWidth = 120
        };

        var columnWidths5 = gridService.CalculateColumnWidths(newGridConfig, 1000.0);
        
        Console.WriteLine($"New Config: 1000px, {newGridConfig.ColumnCount} columns");
        Console.WriteLine($"Column widths: [{string.Join(", ", Array.ConvertAll(columnWidths5, x => x.ToString("F1")))}]");
        Console.WriteLine($"Total width: {columnWidths5.Sum():F1}px\n");

        Console.WriteLine("With Different Column Count (7->5):");
        foreach (var panel in testPanels)
        {
            // Use original grid position but new column count
            var startCol = gridService.CalculateStartColumn(panel.X, originalColumnWidths);
            var span = gridService.CalculateColumnSpan(panel.Width, originalColumnWidths);
            
            // Adjust if columns exceed new count
            var adjustedStartCol = Math.Min(startCol, newGridConfig.ColumnCount - 1);
            var maxSpan = newGridConfig.ColumnCount - adjustedStartCol;
            var adjustedSpan = Math.Min(span, maxSpan);
            
            var newX = gridService.CalculatePositionForColumn(adjustedStartCol, columnWidths5);
            var newWidthCalc = gridService.CalculateWidthForColumnSpan(adjustedSpan, columnWidths5);
            
            Console.WriteLine($"  {panel.Name}: {panel.X:F0}px -> {newX:F0}px, {panel.Width:F0}px -> {newWidthCalc:F0}px [Col {adjustedStartCol}, Span {adjustedSpan}]");
        }

        Console.WriteLine("\nTest completed!");
    }
}