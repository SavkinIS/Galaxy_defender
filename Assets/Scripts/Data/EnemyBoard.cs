using System.Collections.Generic;
using UnityEngine;

public class EnemyBoard
{
    public const int Empty = LevelDataSO.CellData.Empty;

    private readonly int[,,] _cells;

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

    public static EnemyBoard FromLevel(LevelDataSO level)
    {
        int width = Mathf.Max(1, level.Width);
        int height = Mathf.Max(1, level.Height);
        var board = new EnemyBoard(width, height, 1);

        if (level.Grid == null)
            return board;

        foreach (var cell in level.Grid)
        {
            if (cell.HasColor == false)
                continue;

            if (cell.X < 0 || cell.X >= width || cell.Y < 0 || cell.Y >= height)
                continue;

            board._cells[cell.X, cell.Y, 0] = cell.ColorIndex;
            board.RemainingTotal++;
        }

        return board;
    }

    public bool IsExposed(int x, int y, int z)
    {
        if (IsInside(x, y, z) == false)
            return false;

        if (_cells[x, y, z] == Empty)
            return false;

        for (int above = z + 1; above < Layers; above++)
        {
            if (_cells[x, y, above] != Empty)
                return false;
        }

        return true;
    }

    public int ExposedCount(int colorIndex)
    {
        int count = 0;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Layers; z++)
                {
                    if (_cells[x, y, z] == colorIndex && IsExposed(x, y, z))
                        count++;
                }
            }
        }

        return count;
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

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Layers; z++)
                {
                    int color = _cells[x, y, z];
                    if (color == Empty || IsExposed(x, y, z) == false)
                        continue;

                    if (colors.Contains(color) == false)
                        colors.Add(color);
                }
            }
        }

        return colors;
    }

    public int RemoveExposed(int colorIndex, int count)
    {
        if (count <= 0)
            return 0;

        int removed = 0;

        for (int z = Layers - 1; z >= 0 && removed < count; z--)
        {
            for (int y = 0; y < Height && removed < count; y++)
            {
                for (int x = 0; x < Width && removed < count; x++)
                {
                    if (_cells[x, y, z] != colorIndex || IsExposed(x, y, z) == false)
                        continue;

                    _cells[x, y, z] = Empty;
                    removed++;
                    RemainingTotal--;
                }
            }
        }

        return removed;
    }

    private bool IsInside(int x, int y, int z)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Layers;
    }
}
