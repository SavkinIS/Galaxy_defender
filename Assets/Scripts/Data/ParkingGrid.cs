using System.Collections.Generic;
using UnityEngine;
using Utilites;
using Random = UnityEngine.Random;

public class ParkingGrid : MonoBehaviour
{
    [SerializeField] private int _sizeX = 10;
    [SerializeField] private int _sizeY = 10;
    [SerializeField] private List<ParkingCell> _cellsView;
    [SerializeField] private Transform _conteiner;

    private bool[,] _cells;
    private bool _hasOccupiedCells;

    private readonly (int x, int y)[] _directions =
    {
        (1, 0),
        (-1, 0),
        (0, 1),
        (0, -1)
    };

    private Dictionary<(int x, int y), ParkingCell> _cellsViewMap;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_cells == null)
        {
            _cells = new bool[_sizeX, _sizeY];
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                    _cells[x, y] = true;
            }

            _hasOccupiedCells = false;
        }

        if (_cellsViewMap == null && _cellsView != null && _cellsView.Count > 0)
        {
            _cellsViewMap = new Dictionary<(int x, int y), ParkingCell>(_cellsView.Count);
            foreach (var cell in _cellsView)
                _cellsViewMap[(cell.X, cell.Y)] = cell;
        }
    }

    public bool IsCellFree(int x, int y)
    {
        if (x < 0 || x >= _sizeX || y < 0 || y >= _sizeY)
            return false;

        return _cells[x, y];
    }

    private bool CanPlaceShip(int startX, int startY, int endX, int endY)
    {
        if (TryGetShipCells(startX, startY, endX, endY, out var cells) == false)
            return false;

        for (int i = 0; i < cells.Count; i++)
        {
            if (IsCellFree(cells[i].x, cells[i].y) == false)
                return false;
        }

        return true;
    }

    private void PlaceShip(int startX, int startY, int endX, int endY)
    {
        if (CanPlaceShip(startX, startY, endX, endY) == false)
            return ;

        TryGetShipCells(startX, startY, endX, endY, out var cells);

        for (int i = 0; i < cells.Count; i++)
        {
            _cells[cells[i].x, cells[i].y] = false;
        }

        _hasOccupiedCells = true;
    }

    public bool TryPlaceShip(ShipData shipData, ShipView shipPrefab)
    {
        EnsureInitialized();

        int size = Mathf.Max(1, shipData.Size);
        List<(int x, int y)> seeds = GetPlacementSeeds();
        seeds.Shuffle();

        for (int i = 0; i < seeds.Count; i++)
        {
            if (TryPlaceFromCell(shipData, shipPrefab, seeds[i].x, seeds[i].y, size))
                return true;
        }

        return TryPlaceAnywhere(shipData, shipPrefab, size);
    }

    private bool TryPlaceFromCell(ShipData shipData, ShipView shipPrefab, int startX, int startY, int size)
    {
        (int x, int y)[] orderedDirections = GetPreferredDirections(startX, startY);

        for (int i = 0; i < orderedDirections.Length; i++)
        {
            int endX = startX + orderedDirections[i].x * (size - 1);
            int endY = startY + orderedDirections[i].y * (size - 1);

            if (CanPlaceShip(startX, startY, endX, endY) == false)
                continue;

            PlaceShip(startX, startY, endX, endY);
            SpawnShipView(shipData, shipPrefab, endX, endY, orderedDirections[i]);
            return true;
        }

        return false;
    }

    private bool TryPlaceAnywhere(ShipData shipData, ShipView shipPrefab, int size)
    {
        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
            {
                if (IsCellFree(x, y) == false)
                    continue;

                if (TryPlaceFromCell(shipData, shipPrefab, x, y, size))
                    return true;
            }
        }

        return false;
    }

    private List<(int x, int y)> GetPlacementSeeds()
    {
        if (_hasOccupiedCells == false)
            return GetCenterCells();

        List<(int x, int y)> boundary = GetRandomBoundaryCells();
        return boundary.Count > 0 ? boundary : GetCenterCells();
    }

    private List<(int x, int y)> GetCenterCells()
    {
        var cells = new List<(int x, int y)>();
        int midX = _sizeX / 2;
        int midY = _sizeY / 2;

        for (int x = midX - 1; x <= midX; x++)
        {
            for (int y = midY - 1; y <= midY; y++)
            {
                if (IsCellFree(x, y))
                    cells.Add((x, y));
            }
        }

        if (cells.Count == 0)
        {
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    if (IsCellFree(x, y))
                        cells.Add((x, y));
                }
            }
        }

        return cells;
    }

    private List<(int x, int y)> GetRandomBoundaryCells()
    {
        var freeNeighbors = new List<(int x, int y)>();

        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
            {
                if (_cells[x, y])
                    continue;

                for (int i = 0; i < _directions.Length; i++)
                {
                    int nx = x + _directions[i].x;
                    int ny = y + _directions[i].y;

                    if (IsCellFree(nx, ny) && freeNeighbors.Contains((nx, ny)) == false)
                        freeNeighbors.Add((nx, ny));
                }
            }
        }

        return freeNeighbors;
    }

    private (int x, int y)[] GetPreferredDirections(int startX, int startY)
    {
        var ordered = new (int x, int y)[_directions.Length];
        for (int i = 0; i < _directions.Length; i++)
            ordered[i] = _directions[i];

        ordered.Shuffle();

        bool preferPositiveX = startX < _sizeX / 2;
        bool preferPositiveY = startY < _sizeY / 2;

        System.Array.Sort(ordered, (a, b) =>
        {
            int Score((int x, int y) d)
            {
                int score = 0;
                if (d.x != 0)
                    score += (d.x > 0) == preferPositiveX ? 0 : 1;
                if (d.y != 0)
                    score += (d.y > 0) == preferPositiveY ? 0 : 1;
                return score;
            }

            return Score(a).CompareTo(Score(b));
        });

        return ordered;
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
            _cellsViewMap[(cell.X, cell.Y)] = cell;
    }

    private ShipView SpawnShipView(
        ShipData shipData,
        ShipView shipPrefab,
        int frontX,
        int frontY,
        (int x, int y) facing)
    {
        EnsureInitialized();

        Vector3 frontPos = _cellsViewMap[(frontX, frontY)].transform.position;
        Vector3 forward = GetFacingWorldAxis(facing);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, forward);

        Transform parent = _conteiner != null ? _conteiner : transform;
        ShipView shipInstance = Instantiate(shipPrefab, frontPos, rotation, parent);
        shipInstance.SetData(shipData);

        float cellSize = GetGridAxisX().magnitude;
        if (cellSize < 0.01f)
            cellSize = 2.5f;

        shipInstance.transform.localScale = Vector3.one * cellSize;
        return shipInstance;
    }

    private Vector3 GetFacingWorldAxis((int x, int y) facing)
    {
        Vector3 axis = facing.x != 0
            ? GetGridAxisX() * Mathf.Sign(facing.x)
            : GetGridAxisY() * Mathf.Sign(facing.y);

        axis.y = 0f;

        if (axis.sqrMagnitude < 0.0001f)
            axis = transform.right;

        return axis.normalized;
    }

    private Vector3 GetGridAxisX()
    {
        if (_cellsViewMap != null &&
            _cellsViewMap.TryGetValue((0, 0), out var a) &&
            _cellsViewMap.TryGetValue((1, 0), out var b))
        {
            return b.transform.position - a.transform.position;
        }

        return transform.right;
    }

    private Vector3 GetGridAxisY()
    {
        if (_cellsViewMap != null &&
            _cellsViewMap.TryGetValue((0, 0), out var a) &&
            _cellsViewMap.TryGetValue((0, 1), out var b))
        {
            return b.transform.position - a.transform.position;
        }

        return transform.forward;
    }

    private static bool TryGetShipCells(int startX, int startY, int endX, int endY, out List<(int x, int y)> cells)
    {
        cells = new List<(int x, int y)>();

        bool sameX = startX == endX;
        bool sameY = startY == endY;

        if (sameX == false && sameY == false)
            return false;

        if (sameX && sameY)
        {
            cells.Add((startX, startY));
            return true;
        }

        if (sameX)
        {
            int steps = Mathf.Abs(startY - endY) + 1;
            int y = Mathf.Min(startY, endY);
            for (int i = 0; i < steps; i++)
                cells.Add((startX, y + i));
        }
        else
        {
            int steps = Mathf.Abs(startX - endX) + 1;
            int x = Mathf.Min(startX, endX);
            for (int i = 0; i < steps; i++)
                cells.Add((x + i, startY));
        }

        return true;
    }

}