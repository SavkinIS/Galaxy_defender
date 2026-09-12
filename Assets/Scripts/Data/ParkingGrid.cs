using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParkingGrid : MonoBehaviour
{
    [SerializeField] private int _sizeX = 10;
    [SerializeField] private int _sizeY = 10;
    [SerializeField] private List<ParkingCell> _cellsView;
    [SerializeField] private Transform _conteiner;

    private bool[,] _cells;
    private (int, int) _lastCell;

    private readonly (int x, int y)[] _directions = new (int, int)[]
    {
        (0, 1),   
        (0, -1), 
        (-1, 0),  
        (1, 0)    
    };

    private Dictionary<(int x, int y), ParkingCell> _cellsViewMap;

    private void Awake()
    {
        _cells = new bool[_sizeX, _sizeY];
        _lastCell = (-1, -1);

        for (var index0 = 0; index0 < _cells.GetLength(0); index0++)
        {
            for (var index1 = 0; index1 < _cells.GetLength(1); index1++)
            {
                _cells[index0, index1] = true;
            }
        }

        if (_cellsViewMap == null && _cellsView != null && _cellsView.Count > 0)
        {
            _cellsViewMap = _cellsView.ToDictionary(c => (c.X, c.Y));
        }
    }

    public bool IsCellFree(int x, int y)
    {
        if (x < 0 || x >= _sizeX || y < 0 || y >= _sizeY)
            return false;
        
        return _cells[x, y];
    }
    
    public bool CanPlaceShip(int startX, int startY, int endX, int endY)
    {
        bool isYLineCheck = startX == endX;
        bool isXLineCheck = startY == endY;
        bool isFree = false;
        
        if (isYLineCheck && isXLineCheck)
        {
            return IsCellFree(startX, startY);
        }
        else if (isYLineCheck)
        {
            var steps = Steps(startY, endY, out var minValue);

            for (int i = 0; i < steps; i++)
            {
                isFree = IsCellFree(startX, minValue);
                minValue++;
                
                if (isFree == false)
                    return false;
            }
            
            return isFree;
        }
        else
        {
            var steps = Steps(startX, endX, out var minValue);

            for (int i = 0; i < steps; i++)
            {
                isFree = IsCellFree(minValue, startY);
                minValue++;
                
                if (isFree == false)
                    return false;
            }
            
            return isFree;
        }
    }
    
    public bool PlaceShip(int startX, int startY, int endX, int endY)
    {
        bool isYLineCheck = startX == endX;
        bool isXLineCheck = startY == endY;
        
        List<(int x, int y)> cells = new List<(int x, int y)>();
        
        if (isYLineCheck && isXLineCheck)
        {
            if (IsCellFree(startX, startY) == false)
                return false;
            
            cells.Add((startX, startY));
        }
        else if (isYLineCheck)
        {
            var steps = Steps(startY, endY, out var minValue);

            for (int i = 0; i < steps; i++)
            {
                bool isFree = IsCellFree(startX, minValue);
                
                if (isFree == false)
                    return false;
                
                cells.Add((startX, minValue));
                minValue++;
            }
        }
        else
        {
            var steps = Steps(startX, endX, out var minValue);

            for (int i = 0; i < steps; i++)
            {
                bool isFree = IsCellFree(minValue, startY);
                
                if (isFree == false)
                    return false;
                
                cells.Add((minValue, startY));
                minValue++;
            }
        }

        foreach (var cell in cells)
        {
            _cells[cell.x, cell.y] = false;
        }
        
        return true;
    }

    private static int Steps(int startX, int endX, out int minValue)
    {
        int steps = Mathf.Abs(startX - endX) + 1;
        minValue = Mathf.Min(startX, endX);
        return steps;
    }

    public bool TryPlaceShip(ShipData shipData, ShipView shipPrefab)
    {
        (int x, int y) currentCell;
        
        // 1. Берем начальную клетку
        if (_lastCell == (-1, -1))
        {
            _lastCell = (Random.Range(_sizeX / 2 - 1, _sizeX / 2 + 1), Random.Range(_sizeY / 2 - 1, _sizeY / 2 + 1));
            currentCell = _lastCell;
        }
        else
        {
            currentCell = GetRandomBoundaryCell();
            
            if (currentCell.x == -1 || currentCell.y == -1)
                return false;
        }

        int startX = currentCell.x;
        int startY = currentCell.y;
        int endX = startX;
        int endY = startY;

        // 2. Рассчитываем конечную клетку в зависимости от половины сетки (<= 4)
        bool isHorizontal = Random.Range(0, 2) == 0;

        if (isHorizontal)
        {
            if (startX <= 4)
                endX = startX + shipData.Size - 1;
            else
                endX = startX - shipData.Size + 1;
        }
        else
        {
            if (startY <= 4)
                endY = startY + shipData.Size - 1;
            else
                endY = startY - shipData.Size + 1;
        }

        // 3. Проверяем свободны ли клетки и размещаем
        if (CanPlaceShip(startX, startY, endX, endY))
        {
            PlaceShip(startX, startY, endX, endY);

            // 4. Упорядочиваем координаты от меньшего к большему для правильного спавна
            int minX = Mathf.Min(startX, endX);
            int maxX = Mathf.Max(startX, endX);
            int minY = Mathf.Min(startY, endY);
            int maxY = Mathf.Max(startY, endY);

            SpawnShipView(shipData, shipPrefab, minX, minY, maxX, maxY);
            return true;
        }
        
        return false;
    }

    private (int x, int y) GetRandomBoundaryCell()
    {
        List<(int x, int y)> occupiedCells = new List<(int x, int y)>();
        
        for (var index0 = 0; index0 < _cells.GetLength(0); index0++)
        {
            for (var index1 = 0; index1 < _cells.GetLength(1); index1++)
            {
                if (_cells[index0, index1] == false)
                {
                    (int x, int y) cell = (index0, index1);

                    for (int i = 0; i < _directions.Length; i++)
                    {
                        (int x, int y) neighbor = (_directions[i].x + cell.x, _directions[i].y + cell.y);
                        
                        if (IsCellFree(neighbor.x, neighbor.y))
                        {
                            if (occupiedCells.Contains(neighbor) == false)
                                occupiedCells.Add((neighbor.x, neighbor.y));
                        }
                    }
                }
            }
        }
        
        if (occupiedCells.Count > 0)
            return occupiedCells[Random.Range(0, occupiedCells.Count)];
        else
            return (-1, -1);
    }

    public void SetSize(int columns, int rows)
    {
        _sizeX = columns;
        _sizeY = rows;
    }

    public void SetCells(List<ParkingCell> prefabs)
    {
        _cellsView = new List<ParkingCell>(prefabs);
    
        _cellsViewMap = new Dictionary<(int x, int y), ParkingCell>();
        foreach (var cell in _cellsView)
        {
            _cellsViewMap[(cell.X, cell.Y)] = cell;
        }
    }
    
    public ShipView SpawnShipView(
        ShipData shipData,
        ShipView shipPrefab,
        int startX,
        int startY,
        int endX,
        int endY)
    {
        // Нормализуем координаты.
        // Для горизонтального корабля start всегда слева, end всегда справа.
        // Для вертикального корабля start всегда снизу, end всегда сверху.

        if (startY == endY && startX > endX)
        {
            (startX, endX) = (endX, startX);
        }
        else if (startX == endX && startY > endY)
        {
            (startY, endY) = (endY, startY);
        }

        Vector3 startPos = _cellsViewMap[(startX, startY)].transform.position;
        Vector3 endPos = _cellsViewMap[(endX, endY)].transform.position;

        // Центр всех занимаемых клеток
        Vector3 centerPos = (startPos + endPos) / 2f;

        // Определяем, горизонтальный корабль или вертикальный
        bool isHorizontal = startY == endY;

        Vector3 lookDirection;

        if (isHorizontal)
        {
            // Центр корабля ближе к левой или правой границе?
            float distanceToLeft = centerPos.x - _cellsViewMap[(0, startY)].transform.position.x;
            float distanceToRight = _cellsViewMap[(_sizeX - 1, startY)].transform.position.x - centerPos.x;

            lookDirection = distanceToLeft <= distanceToRight
                ? Vector3.left
                : Vector3.right;
        }
        else
        {
            // Центр корабля ближе к нижней или верхней границе?
            float distanceToBottom = centerPos.y - _cellsViewMap[(startX, 0)].transform.position.y;
            float distanceToTop = _cellsViewMap[(startX, _sizeY - 1)].transform.position.y - centerPos.y;

            lookDirection = distanceToBottom <= distanceToTop
                ? Vector3.down
                : Vector3.up;
        }

        Quaternion rotation = Quaternion.LookRotation(lookDirection);

        ShipView shipInstance = Instantiate(
            shipPrefab,
            centerPos,
            rotation,
            _conteiner
        );

        shipInstance.SetData(shipData);

        return shipInstance;
    }
}