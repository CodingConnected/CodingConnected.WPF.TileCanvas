using System;
using System.Linq;
using System.Windows;
using CodingConnected.WPF.TileCanvas.Library.Enums;
using CodingConnected.WPF.TileCanvas.Library.Models;

namespace CodingConnected.WPF.TileCanvas.Library.Services
{
    /// <summary>
    /// Service for grid-related calculations and operations
    /// </summary>
    public class GridCalculationService
    {
        private double[]? _columnWidths;

        /// <summary>
        /// Calculates column widths for flexible grid mode
        /// </summary>
        public double[] CalculateColumnWidths(GridConfiguration config, double availableWidth)
        {
            if (config.Mode != GridMode.Flexible)
                throw new InvalidOperationException("Column width calculation is only valid for flexible grid mode");

            var columnWidths = new double[config.ColumnCount];
            var totalMinWidth = config.ColumnCount * config.MinColumnWidth;

            if (availableWidth >= totalMinWidth)
            {
                // We have sufficient space, distribute it equally among columns
                var columnWidth = availableWidth / config.ColumnCount;
                for (int i = 0; i < config.ColumnCount; i++)
                {
                    columnWidths[i] = columnWidth;
                }
            }
            else
            {
                // Insufficient space - use minimum width, horizontal scrollbar will appear
                for (int i = 0; i < config.ColumnCount; i++)
                {
                    columnWidths[i] = config.MinColumnWidth;
                }
            }

            _columnWidths = columnWidths;
            return columnWidths;
        }

        /// <summary>
        /// Snaps a position to the nearest grid point
        /// </summary>
        public Point SnapToGrid(Point position, GridConfiguration config, double canvasWidth)
        {
            if (config.Mode == GridMode.Fixed)
            {
                return SnapToFixedGrid(position, config.GridSize);
            }
            else
            {
                return SnapToFlexibleGrid(position, config, canvasWidth);
            }
        }

        /// <summary>
        /// Snaps a size to valid grid dimensions (or applies proportional sizing for flexible mode)
        /// </summary>
        public Size SnapSizeToGrid(Size size, GridConfiguration config, double canvasWidth)
        {
            if (config.Mode == GridMode.Fixed)
            {
                return SnapSizeToFixedGrid(size, config.GridSize);
            }
            else
            {
                // In flexible mode, snap to column boundaries
                return SnapSizeToFlexibleGrid(size, config, canvasWidth);
            }
        }

        /// <summary>
        /// Calculates how many columns a given width spans in flexible mode
        /// </summary>
        public int CalculateColumnSpan(double width, double[] columnWidths)
        {
            double totalWidth = 0;
            int span = 1;

            for (int i = 0; i < columnWidths.Length; i++)
            {
                totalWidth += columnWidths[i];
                if (Math.Abs(width - totalWidth) < 5) // Small tolerance for rounding
                {
                    span = i + 1;
                    break;
                }
            }

            return span;
        }

        /// <summary>
        /// Calculates width for a given number of columns
        /// </summary>
        public double CalculateWidthForColumnSpan(int columnSpan, double[] columnWidths)
        {
            if (columnSpan <= 0) return columnWidths.FirstOrDefault();

            double width = 0;
            for (int i = 0; i < Math.Min(columnSpan, columnWidths.Length); i++)
            {
                width += columnWidths[i];
            }

            return width;
        }

        /// <summary>
        /// Finds the starting column index for a given X position
        /// </summary>
        public int CalculateStartColumn(double xPosition, double[] columnWidths)
        {
            double totalWidth = 0;

            for (int i = 0; i < columnWidths.Length; i++)
            {
                if (xPosition <= totalWidth + 5) // Small tolerance for rounding
                {
                    return i;
                }
                totalWidth += columnWidths[i];
            }

            return Math.Max(0, columnWidths.Length - 1);
        }

        /// <summary>
        /// Calculates X position for a given column index
        /// </summary>
        public double CalculatePositionForColumn(int columnIndex, double[] columnWidths)
        {
            if (columnIndex <= 0) return 0;

            double position = 0;
            for (int i = 0; i < Math.Min(columnIndex, columnWidths.Length); i++)
            {
                position += columnWidths[i];
            }

            return position;
        }

        private Point SnapToFixedGrid(Point position, int gridSize)
        {
            var snappedX = Math.Round(position.X / gridSize) * gridSize;
            var snappedY = Math.Round(position.Y / gridSize) * gridSize;
            return new Point(snappedX, snappedY);
        }

        private Point SnapToFlexibleGrid(Point position, GridConfiguration config, double canvasWidth)
        {
            // Always recalculate column widths to ensure consistency with current canvas width
            var columnWidths = CalculateColumnWidths(config, canvasWidth);

            // Snap horizontally to column boundaries
            double snappedX = 0;
            double currentX = 0;

            for (int i = 0; i < config.ColumnCount; i++)
            {
                double columnEnd = currentX + columnWidths[i];

                if (position.X <= columnEnd)
                {
                    double distToStart = Math.Abs(position.X - currentX);
                    double distToEnd = Math.Abs(position.X - columnEnd);
                    snappedX = distToStart <= distToEnd ? currentX : columnEnd;
                    break;
                }

                currentX = columnEnd;
            }

            // Snap vertically to regular grid
            var snappedY = Math.Round(position.Y / config.GridSize) * config.GridSize;

            return new Point(snappedX, snappedY);
        }

        private Size SnapSizeToFixedGrid(Size size, int gridSize)
        {
            var snappedWidth = Math.Round(size.Width / gridSize) * gridSize;
            var snappedHeight = Math.Round(size.Height / gridSize) * gridSize;

            snappedWidth = Math.Max(gridSize * 2, snappedWidth);
            snappedHeight = Math.Max(gridSize * 2, snappedHeight);

            return new Size(snappedWidth, snappedHeight);
        }

        /// <summary>
        /// Snaps size to flexible grid column boundaries
        /// </summary>
        private Size SnapSizeToFlexibleGrid(Size size, GridConfiguration config, double canvasWidth)
        {
            // Always recalculate column widths to ensure consistency with current canvas width
            var columnWidths = CalculateColumnWidths(config, canvasWidth);

            // Find the best column span by calculating the closest match
            double bestWidth = columnWidths[0]; // Minimum: 1 column
            double bestDifference = Math.Abs(size.Width - bestWidth);
            double totalWidth = 0;

            for (int i = 0; i < config.ColumnCount; i++)
            {
                totalWidth += columnWidths[i];
                double difference = Math.Abs(size.Width - totalWidth);

                // Choose this column span if it's closer to the target size
                if (difference < bestDifference)
                {
                    bestWidth = totalWidth;
                    bestDifference = difference;
                }
            }

            // For height, use regular grid snapping
            var snappedHeight = Math.Round(size.Height / config.GridSize) * config.GridSize;
            snappedHeight = Math.Max(config.GridSize * 2, snappedHeight);

            return new Size(bestWidth, snappedHeight);
        }
    }
}