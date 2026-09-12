using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipsPuzzle : MonoBehaviour
{
    [SerializeField] private ShipView _shipViewS1;
    [SerializeField] private ShipView _shipViewS2;
    [SerializeField] private ShipView _shipViewS3;
    [SerializeField] private ShipParkingCreator _parkingCreator;
    [SerializeField] private ParkingGrid _parkingGrid;
    [SerializeField] private LevelDataSO _levelData;
    
    private Dictionary<int, ShipView> _shipsById;
    private Dictionary<int, List<LevelDataSO.CellData>> _grids;
    private Dictionary<Color, List<ShipData>> _dataByColor;
    private List<ShipData> _dataColor;
    private List<Color> _colors;
    private int _index = 0;

    private void Start()
    {
        _shipsById = new Dictionary<int, ShipView>()
        {
            { 1, _shipViewS1 },
            { 2, _shipViewS2 },
            { 3, _shipViewS3 },
        };

        _grids = new (_levelData.ColorMappings.Length);
        _dataByColor = new();
        
        foreach (var cell in _levelData.Grid)
        {
            if (cell.HasColor)
            {
                if (_grids.ContainsKey(cell.ColorIndex))
                {
                    _grids[cell.ColorIndex].Add(cell);
                }
                else
                {
                    _grids[cell.ColorIndex] = new List<LevelDataSO.CellData>();
                    _grids[cell.ColorIndex].Add(cell);
                }
            }
        }

        _grids.Remove(LevelDataSO.CellData.Empty);

        foreach (var grid in _grids)
        {
           var colrIndex = grid.Key;
           Color color = _levelData.GetColor(colrIndex);
           var data = _parkingCreator.GenerateShipsForColor(grid.Value.Count, 2, color, colrIndex);
           _dataByColor.Add(color, data);
        }

        _colors = new( _dataByColor.Keys);
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_index >= _colors.Count)
                return;
            
            for (var i = 0; i < _dataByColor[_colors[_index]].Count; i++)
            {
                var data = _dataByColor[_colors[_index]][i];
                if (_parkingGrid.TryPlaceShip(data, _shipsById[data.Size]) == false)
                    Debug.LogWarning($"Не удалось разместить корабль размером {data.Size} на сетке!");
            }

            _index++;
        }
    }
}