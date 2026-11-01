using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMap : MonoBehaviour
{
    [SerializeField] private List<Cell> _cells =  new List<Cell>();
    [SerializeField] private LevelDataSO _levelData;

    private struct CellLink
    {
        public Cell Cell;
        public int ColorIndex;
        public bool IsActive;
        public int Left;
        public int Right;
        public int Down;
        public int Up;
    }

    private CellLink[] _cellLinks;
    private int[] _firstInRows;
    private int[] _lastInRows;
    private int[] _firstInColumns;
    private int[] _lastInColumns;
    private int _width;
    private int _height;
    private int _volleyCursor;

    private void Awake()
    {
        _width = _levelData.Width;
        _height = _levelData.Height;
        _cellLinks = new CellLink[_width * _height];

        for (var index = 0; index < _cells.Count; index++)
        {
            var cell = _cells[index];
            var data = _levelData.GetCell(cell.X, cell.Y);

            cell.SetColor(_levelData.GetColor(data.ColorIndex), data.HasColor);

            if (!data.HasColor)
                continue;

            _cellLinks[cell.Y * _width + cell.X] = new CellLink
            {
                Cell = cell,
                ColorIndex = data.ColorIndex,
                IsActive = true,
                Left = -1,
                Right = -1,
                Down = -1,
                Up = -1
            };
        }

        BuildLineIndex();
    }

    private void BuildLineIndex()
    {
        _firstInRows = new int[_height];
        _lastInRows = new int[_height];
        _firstInColumns = new int[_width];
        _lastInColumns = new int[_width];

        for (var x = 0; x < _width; x++)
        {
            _firstInColumns[x] = -1;
            _lastInColumns[x] = -1;
        }

        for (var y = 0; y < _height; y++)
        {
            _firstInRows[y] = -1;
            _lastInRows[y] = -1;

            for (var x = 0; x < _width; x++)
            {
                var cellIndex = y * _width + x;

                if (!_cellLinks[cellIndex].IsActive)
                    continue;

                var left = _lastInRows[y];
                var down = _lastInColumns[x];
                _cellLinks[cellIndex].Left = left;
                _cellLinks[cellIndex].Down = down;

                if (left >= 0)
                    _cellLinks[left].Right = cellIndex;
                else
                    _firstInRows[y] = cellIndex;

                if (down >= 0)
                    _cellLinks[down].Up = cellIndex;
                else
                    _firstInColumns[x] = cellIndex;

                _lastInRows[y] = cellIndex;
                _lastInColumns[x] = cellIndex;
            }
        }
    }
    
    public void SetCell(Cell cell)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        if (_cells.Contains(cell) == false)
        {
            _cells.Add(cell);
        }
    }

    public bool TryGetTargetCellIndex((int x, int y) startCellIndex,
        (int x, int y) direction, int colorIndex, out (int x, int y) targetCellIndex)
    {
        targetCellIndex = (-1, -1);

        if (startCellIndex.x < 0 || startCellIndex.x >= _width ||
            startCellIndex.y < 0 || startCellIndex.y >= _height)
            return false;

        int cellIndex;

        // Точки коннектора задают вход с границы, поэтому достаточно конца цепочки.
        if (direction.x == 1 && direction.y == 0 && startCellIndex.x == 0)
            cellIndex = _firstInRows[startCellIndex.y];
        else if (direction.x == -1 && direction.y == 0 && startCellIndex.x == _width - 1)
            cellIndex = _lastInRows[startCellIndex.y];
        else if (direction.x == 0 && direction.y == 1 && startCellIndex.y == 0)
            cellIndex = _firstInColumns[startCellIndex.x];
        else if (direction.x == 0 && direction.y == -1 && startCellIndex.y == _height - 1)
            cellIndex = _lastInColumns[startCellIndex.x];
        else
            return false;

        if (cellIndex < 0 || _cellLinks[cellIndex].ColorIndex != colorIndex)
            return false;

        targetCellIndex = (cellIndex % _width, cellIndex / _width);
        return true;
    }

    public void DestroyCell((int x, int y) targetCellIndex)
    {
        if (targetCellIndex.x < 0 || targetCellIndex.x >= _width ||
            targetCellIndex.y < 0 || targetCellIndex.y >= _height)
            return;

        var cellIndex = targetCellIndex.y * _width + targetCellIndex.x;
        var cell = _cellLinks[cellIndex];

        if (!cell.IsActive)
            return;

        _cellLinks[cellIndex].IsActive = false;

        if (cell.Left >= 0)
            _cellLinks[cell.Left].Right = cell.Right;
        else
            _firstInRows[targetCellIndex.y] = cell.Right;

        if (cell.Right >= 0)
            _cellLinks[cell.Right].Left = cell.Left;
        else
            _lastInRows[targetCellIndex.y] = cell.Left;

        if (cell.Down >= 0)
            _cellLinks[cell.Down].Up = cell.Up;
        else
            _firstInColumns[targetCellIndex.x] = cell.Up;

        if (cell.Up >= 0)
            _cellLinks[cell.Up].Down = cell.Down;
        else
            _lastInColumns[targetCellIndex.x] = cell.Down;

        // Обновляем индекс до Disable: OnDisable может запустить следующий запрос.
        cell.Cell.Disable();
    }

    /// <summary>
    /// Destroys the next active block of the given color, walking the grid with a wrapping cursor
    /// so consecutive shots do not eat a single row. Blocks buried inside a chain are valid
    /// targets here, unlike <see cref="TryGetTargetCellIndex"/>.
    /// </summary>
    public bool TryDestroyAnyCell(int colorIndex)
    {
        if (_cellLinks == null || _cellLinks.Length == 0)
            return false;

        var total = _cellLinks.Length;

        for (var offset = 0; offset < total; offset++)
        {
            var index = (_volleyCursor + offset) % total;

            if (!_cellLinks[index].IsActive || _cellLinks[index].ColorIndex != colorIndex)
                continue;

            _volleyCursor = (index + 1) % total;
            var cell = _cellLinks[index].Cell;
            DestroyCell((cell.X, cell.Y));
            return true;
        }

        return false;
    }

    public int DestroyColor(int colorIndex)
    {
        if (_cellLinks == null)
            return 0;

        var destroyed = 0;

        for (var index = 0; index < _cellLinks.Length; index++)
        {
            if (!_cellLinks[index].IsActive || _cellLinks[index].ColorIndex != colorIndex)
                continue;

            var cell = _cellLinks[index].Cell;
            DestroyCell((cell.X, cell.Y));
            destroyed++;
        }

        return destroyed;
    }

    public bool TryGetColorIndex(Cell cell, out int colorIndex)
    {
        colorIndex = -1;

        if (cell == null || _cellLinks == null)
            return false;

        if (cell.X < 0 || cell.X >= _width || cell.Y < 0 || cell.Y >= _height)
            return false;

        var index = cell.Y * _width + cell.X;

        if (!_cellLinks[index].IsActive)
            return false;

        colorIndex = _cellLinks[index].ColorIndex;
        return true;
    }

    public IReadOnlyList<int> GetActiveColors()
    {
        var colors = new List<int>();

        if (_cellLinks == null)
            return colors;

        for (var index = 0; index < _cellLinks.Length; index++)
        {
            if (!_cellLinks[index].IsActive)
                continue;

            var colorIndex = _cellLinks[index].ColorIndex;
            if (!colors.Contains(colorIndex))
                colors.Add(colorIndex);
        }

        return colors;
    }
}