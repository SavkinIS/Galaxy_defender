using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMap : MonoBehaviour
{
    [SerializeField] private List<Cell> _cells =  new List<Cell>();
    [SerializeField] private LevelDataSO _levelData;

    private List<Cell> _activeCells = new List<Cell>();
    
    private Dictionary<(int x, int y), Cell> _cellMap = new();
    
    private void Awake()
    {
        for (var index = 0; index < _cells.Count; index++)
        {
            var cell = _cells[index];
            var data = _levelData.GetCell(cell.X, cell.Y);
            
            cell.SetColor(_levelData.GetColor(data.ColorIndex), data.HasColor);
            
            if (data.HasColor)
            {
                _activeCells.Add(cell);

                _cellMap[(cell.X, cell.Y)] =  cell;
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

        if (direction.x == 0 && direction.y == 0)
            return false;

        var currentCellIndex = startCellIndex;

        while (currentCellIndex.x >= 0 && currentCellIndex.x < _levelData.Width &&
               currentCellIndex.y >= 0 && currentCellIndex.y < _levelData.Height)
        {
            if (_cellMap.TryGetValue(currentCellIndex, out var cell) && cell.IsActive)
            {
                if (_levelData.GetCell(currentCellIndex.x, currentCellIndex.y).ColorIndex != colorIndex)
                    return false;

                targetCellIndex = currentCellIndex;
                return true;
            }

            currentCellIndex = (currentCellIndex.x + direction.x, currentCellIndex.y + direction.y);
        }

        return false;
    }

    public void DestroyCell((int x, int y) targetCellIndex)
    {
        if (_cellMap.TryGetValue(targetCellIndex, out var cell))
        {
            if (cell.IsActive)
                cell.Disable();
        }
    }
}