using System.Collections.Generic;
using UnityEngine;

public class EnemyBoard
{
    public const int Empty = LevelDataSO.CellData.Empty;

    private readonly int[,,] _cells;
    private readonly List<((int x, int y) start, (int x, int y) direction)> _firingLines =
        new List<((int x, int y) start, (int x, int y) direction)>();

    public int Width { get; }
    public int Height { get; }
    public int Layers { get; }
    public int RemainingTotal { get; private set; }

    private EnemyBoard(int width, int height, int layers)
    {
        Width = width;
        Height = height;
        Layers = layers;
        _cells = new int[width, height, layers];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < layers; z++)
                    _cells[x, y, z] = Empty;
            }
        }
    }

    public static EnemyBoard FromLevel(LevelDataSO level,
        IReadOnlyList<PathPoint> battlePoints = null, EnemyCellConnector connector = null)
    {
        int width = Mathf.Max(1, level.Width);
        int height = Mathf.Max(1, level.Height);
        var board = new EnemyBoard(width, height, 1);

        if (battlePoints != null && connector != null)
        {
            for (int index = 0; index < battlePoints.Count; index++)
            {
                if (battlePoints[index] != null &&
                    connector.TryGetLine(battlePoints[index], out var start, out var direction))
                    board._firingLines.Add((start, direction));
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                board._firingLines.Add(((x, 0), (0, 1)));
                board._firingLines.Add(((x, height - 1), (0, -1)));
            }
            for (int y = 0; y < height; y++)
            {
                board._firingLines.Add(((0, y), (1, 0)));
                board._firingLines.Add(((width - 1, y), (-1, 0)));
            }
        }

        if (level.Grid == null)
            return board;

        foreach (var cell in level.Grid)
        {
            if (cell.HasColor == false)
                continue;

            if (cell.X < 0 || cell.X >= width || cell.Y < 0 || cell.Y >= height)
                continue;

            if (board._cells[cell.X, cell.Y, 0] != Empty)
                continue;

            board._cells[cell.X, cell.Y, 0] = cell.ColorIndex;
            board.RemainingTotal++;
        }

        return board;
    }

    public EnemyBoard Clone()
    {
        var clone = new EnemyBoard(Width, Height, Layers)
        {
            RemainingTotal = RemainingTotal
        };
        clone._firingLines.AddRange(_firingLines);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Layers; z++)
                    clone._cells[x, y, z] = _cells[x, y, z];
            }
        }

        return clone;
    }

    public string GetStateKey()
    {
        var key = new System.Text.StringBuilder(Width * Height * Layers + 16);
        key.Append(RemainingTotal).Append('|');

        for (int z = 0; z < Layers; z++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    key.Append(_cells[x, y, z]).Append(',');
            }
        }

        return key.ToString();
    }

    public bool IsExposed(int x, int y, int z)
    {
        return z == 0 && IsInside(x, y, z) && GetExposedCells().Contains((x, y));
    }

    public int ExposedCount(int colorIndex)
    {
        int count = 0;
        foreach (var cell in GetExposedCells())
        {
            if (_cells[cell.x, cell.y, 0] == colorIndex)
                count++;
        }
        return count;
    }

    private HashSet<(int x, int y)> GetExposedCells()
    {
        var cells = new HashSet<(int x, int y)>();
        for (int index = 0; index < _firingLines.Count; index++)
        {
            if (TryGetLineTarget(_firingLines[index], out var target))
                cells.Add(target);
        }
        return cells;
    }

    private bool TryGetLineTarget(
        ((int x, int y) start, (int x, int y) direction) line, out (int x, int y) target)
    {
        target = (-1, -1);
        var start = line.start;
        var direction = line.direction;
        bool entersFromBoundary =
            (direction == (1, 0) && start.x == 0) ||
            (direction == (-1, 0) && start.x == Width - 1) ||
            (direction == (0, 1) && start.y == 0) ||
            (direction == (0, -1) && start.y == Height - 1);
        if (!entersFromBoundary || !IsInside(start.x, start.y, 0))
            return false;

        for (int x = start.x, y = start.y; IsInside(x, y, 0);
             x += direction.x, y += direction.y)
        {
            if (_cells[x, y, 0] == Empty)
                continue;

            target = (x, y);
            return true;
        }
        return false;
    }

    public int RemainingCount(int colorIndex)
    {
        int count = 0;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Layers; z++)
                {
                    if (_cells[x, y, z] == colorIndex)
                        count++;
                }
            }
        }

        return count;
    }

    public List<int> GetExposedColors()
    {
        var colors = new List<int>();
        foreach (var cell in GetExposedCells())
        {
            int color = _cells[cell.x, cell.y, 0];
            if (!colors.Contains(color))
                colors.Add(color);
        }
        return colors;
    }

    public int RemoveExposed(int colorIndex, int count)
    {
        int removed = 0;
        bool fired;
        do
        {
            fired = false;
            for (int index = 0; index < _firingLines.Count && removed < count; index++)
            {
                if (!TryGetLineTarget(_firingLines[index], out var target) ||
                    _cells[target.x, target.y, 0] != colorIndex)
                    continue;

                _cells[target.x, target.y, 0] = Empty;
                RemainingTotal--;
                removed++;
                fired = true;
            }
        }
        while (fired && removed < count);

        return removed;
    }

    private bool IsInside(int x, int y, int z)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Layers;
    }
}
