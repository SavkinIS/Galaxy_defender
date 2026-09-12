using System;
using System.Collections.Generic;
using UnityEngine;

public class CellMap : MonoBehaviour
{
    [SerializeField] private List<Cell> _cells =  new List<Cell>();
    [SerializeField] private LevelDataSO _levelData;

    private List<Cell> _activeCells = new List<Cell>();
    
    private void Awake()
    {
        for (var index = 0; index < _cells.Count; index++)
        {
            var cell = _cells[index];
            var data = _levelData.GetCell(cell.X, cell.Y);
            
            cell.SetColor(_levelData.GetColor(data.ColorIndex), data.HasColor);
            
            if (data.HasColor)
                _activeCells.Add(cell);
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
}