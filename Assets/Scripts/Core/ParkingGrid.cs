using System.Collections.Generic;
using UnityEngine;
using Utilites;

public class ParkingGrid : MonoBehaviour
{
    [SerializeField] private int _sizeX = 10;
    [SerializeField] private int _sizeY = 10;
    [SerializeField] private List<ParkingCell> _cellsView;
    [SerializeField] private Transform _conteiner;

    private bool[,] _cells;
    private bool _hasOccupiedCells;
    private Dictionary<ShipData,ShipView> _createdShips;

    private readonly (int x, int y)[] _directions =
    {
        (1, 0),
        (-1, 0),
        (0, 1),
        (0, -1)
    };

    private Dictionary<(int x, int y), ParkingCell> _cellsViewMap;

    public Dictionary<ShipData, ShipView> CreatedShips => _createdShips;
    
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

    public bool TryPlaceOne(ShipData data, ShipView prefab, int tapeIndex)
    {
        EnsureInitialized();
        return TryPlaceOnRim(data, prefab, tapeIndex);
    }

    public bool TryPlaceTapeReverse(IReadOnlyList<ShipData> tape, System.Func<int, ShipView> prefabBySize)
    {
        _createdShips =  new ();
        
        EnsureInitialized();
        ClearOccupancy();

        Log.Info($"Укладка с конца: кораблей={tape.Count}, сетка={_sizeX}x{_sizeY}, шаг={GetGridAxisX().magnitude:0.00}");

        for (int i = tape.Count - 1; i >= 0; i--)
        {
            ShipData data = tape[i];
            ShipView prefab = prefabBySize(data.Size);

            Log.Info($"Кладу S{i + 1}/{tape.Count} size={data.Size} цвет={data.ColorIndex} ammo={data.MaxBullets}");

            if (prefab == null || TryPlaceOnRim(data, prefab, i) == false)
            {
                Log.Error($"Не удалось уложить S{i + 1} size={data.Size}");
                Log.Info(BuildOccupancyMap());
                return false;
            }
        }

        Log.Info("Итоговая стоянка:\n" + BuildOccupancyMap());
        LogVisualOverlaps();
        return true;
    }

    public bool CanPlaceTapeReverse(IReadOnlyList<ShipData> tape)
    {
        EnsureInitialized();
        bool[,] snapshot = CopyOccupancy();
        ClearOccupancy();

        for (int i = tape.Count - 1; i >= 0; i--)
        {
            ShipData data = tape[i];
            int size = Mathf.Max(1, data.Size);
            if (TryFindRimPlacement(size, true, true, out RimPlacement placement) == false &&
                TryFindRimPlacement(size, false, true, out placement) == false &&
                TryFindRimPlacement(size, false, false, out placement) == false)
            {
                RestoreOccupancy(snapshot);
                return false;
            }

            OccupyFromNose(placement.FrontX, placement.FrontY, placement.Dx, placement.Dy, size);
        }

        RestoreOccupancy(snapshot);
        return true;
    }

    public bool HasClearedExit(RimPlacement placement)
    {
        return HasClearExit(placement.FrontX, placement.FrontY, placement.Dx, placement.Dy);
    }
    
    private bool HasClearExit(int frontX, int frontY, int dx, int dy)
    {
        int x = frontX + dx;
        int y = frontY + dy;

        while (x >= 0 && x < _sizeX && y >= 0 && y < _sizeY)
        {
            if (IsCellFree(x, y) == false)
                return false;

            x += dx;
            y += dy;
        }

        return true;
    }

    private bool[,] CopyOccupancy()
    {
        var snapshot = new bool[_sizeX, _sizeY];
        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
                snapshot[x, y] = _cells[x, y];
        }

        return snapshot;
    }

    private void RestoreOccupancy(bool[,] snapshot)
    {
        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
                _cells[x, y] = snapshot[x, y];
        }

        _hasOccupiedCells = false;
        for (int x = 0; x < _sizeX && _hasOccupiedCells == false; x++)
        {
            for (int y = 0; y < _sizeY; y++)
            {
                if (_cells[x, y] == false)
                {
                    _hasOccupiedCells = true;
                    break;
                }
            }
        }
    }

    public void ClearOccupancy()
    {
        EnsureInitialized();

        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
                _cells[x, y] = true;
        }

        _hasOccupiedCells = false;

        if (_cellsViewMap == null)
            return;

        foreach (var pair in _cellsViewMap)
            pair.Value.SetOccupied(false);
    }

    public void ReleaseOccupiedCells(int frontX, int frontY, int dx, int dy, int size)
    {
        EnsureInitialized();

        for (int i = 0; i < size; i++)
        {
            int x = frontX - dx * i;
            int y = frontY - dy * i;
            if (x < 0 || x >= _sizeX || y < 0 || y >= _sizeY)
                continue;

            _cells[x, y] = true;
            if (_cellsViewMap != null && _cellsViewMap.TryGetValue((x, y), out ParkingCell cell))
                cell.SetOccupied(false);
        }

        _hasOccupiedCells = false;
        for (int x = 0; x < _sizeX && _hasOccupiedCells == false; x++)
        {
            for (int y = 0; y < _sizeY; y++)
            {
                if (_cells[x, y] == false)
                {
                    _hasOccupiedCells = true;
                    break;
                }
            }
        }
    }

    private void DisableOccupiedCells(int frontX, int frontY, int dx, int dy, int size)
    {
        for (int i = 0; i < size; i++)
        {
            int x = frontX - dx * i;
            int y = frontY - dy * i;

            if (_cellsViewMap != null && _cellsViewMap.TryGetValue((x, y), out ParkingCell cell))
                cell.SetOccupied(true);
        }
    }

    public bool TryWorldToCell(Vector3 world, out int x, out int y)
    {
        x = -1;
        y = -1;

        if (_cellsViewMap == null || _cellsViewMap.Count == 0)
            return false;

        float best = float.MaxValue;
        foreach (var pair in _cellsViewMap)
        {
            float dist = (pair.Value.transform.position - world).sqrMagnitude;
            if (dist >= best)
                continue;

            best = dist;
            x = pair.Key.x;
            y = pair.Key.y;
        }

        return x >= 0;
    }

    private bool TryPlaceOnRim(ShipData shipData, ShipView shipPrefab, int tapeIndex)
    {
        int size = Mathf.Max(1, shipData.Size);

        bool withExit = TryFindRimPlacement(size, true, true, out RimPlacement placement);
        Log.Info($"  кандидаты с выездом и склейкой: {(withExit ? $"score={placement.Score} glue={placement.Glue} nose=({placement.FrontX},{placement.FrontY}) dir=({placement.Dx},{placement.Dy})" : "нет")}");

        if (withExit == false)
        {
            bool glued = TryFindRimPlacement(size, false, true, out placement);
            Log.Warn($"  без чистого выезда: {(glued ? $"score={placement.Score} glue={placement.Glue} nose=({placement.FrontX},{placement.FrontY})" : "нет")}");
            withExit = glued;
        }

        if (withExit == false)
        {
            bool any = TryFindRimPlacement(size, false, false, out placement);
            Log.Warn($"  без склейки (дыра): {(any ? $"score={placement.Score} nose=({placement.FrontX},{placement.FrontY})" : "нет")}");
            if (any == false)
                return false;
        }

        if (_cellsViewMap == null || _cellsViewMap.ContainsKey((placement.FrontX, placement.FrontY)) == false)
        {
            Log.Error($"Нет визуальной ячейки для носа ({placement.FrontX},{placement.FrontY})");
            return false;
        }

        OccupyFromNose(placement.FrontX, placement.FrontY, placement.Dx, placement.Dy, size);
        DisableOccupiedCells(placement.FrontX, placement.FrontY, placement.Dx, placement.Dy, size);

        ShipView ship = SpawnShipView(shipData, shipPrefab, placement.FrontX, placement.FrontY, (placement.Dx, placement.Dy));
        if (ship == null)
            return false;
        
        ship.SetPlacement(placement);

        _createdShips[shipData] = ship;
        
        TryWorldToCell(ship.transform.position, out int shipCellX, out int shipCellY);

        Log.Info(
            $"  клетки корабля: {FormatShipCells(placement.FrontX, placement.FrontY, placement.Dx, placement.Dy, size)}");
        Log.Info(
            $"  нос клетки=({placement.FrontX},{placement.FrontY}) dir=({placement.Dx},{placement.Dy}) size={size}");
        Log.Info(
            $"  позиция корабля world={ship.transform.position} в клетках=({shipCellX},{shipCellY})");
        Log.Info("  карта:\n" + BuildOccupancyMap(placement.FrontX, placement.FrontY, placement.Dx, placement.Dy, size));
        return true;
    }

    private bool TryFindRimPlacement(int size, bool requireClearExit, bool requireGlue, out RimPlacement result)
    {
        result = default;
        var found = new List<RimPlacement>();
        GetOccupiedCentroid(out float cx, out float cy, out int occupiedCount);

        for (int d = 0; d < _directions.Length; d++)
        {
            int dx = _directions[d].x;
            int dy = _directions[d].y;

            for (int fx = 0; fx < _sizeX; fx++)
            {
                for (int fy = 0; fy < _sizeY; fy++)
                {
                    if (CanOccupyFromNose(fx, fy, dx, dy, size) == false)
                        continue;

                    if (requireClearExit && HasClearExit(fx, fy, dx, dy) == false)
                        continue;

                    int glue = CountGlue(fx, fy, dx, dy, size);
                    if (requireGlue && occupiedCount > 0 && glue == 0)
                        continue;

                    int emptyInBox = CountEmptyInBoundingBoxAfter(fx, fy, dx, dy, size);
                    int boxArea = BoundingBoxAreaAfter(fx, fy, dx, dy, size);
                    float outward = (fx - cx) * dx + (fy - cy) * dy;
                    int score = glue * 80 - emptyInBox * 35 - boxArea * 2 + Mathf.RoundToInt(outward * 4f);

                    if (occupiedCount == 0)
                    {
                        float dist = Mathf.Abs(fx - cx) + Mathf.Abs(fy - cy);
                        score = 1000 - Mathf.RoundToInt(dist * 20f) + Mathf.RoundToInt(outward * 4f);
                    }

                    int endX = dx == 0 ? -1 : dx > 0 ? _sizeX - 1 : 0;
                    int endY = dy == 0 ? -1 : dy > 0 ? _sizeY - 1 : 0;
                    found.Add(new RimPlacement(fx, fy, dx, dy, endX, endY, score, glue));
                }
            }
        }

        Log.Info($"  найдено вариантов (exit={requireClearExit}, glue={requireGlue}): {found.Count}");

        if (found.Count == 0)
            return false;

        found.Sort((a, b) =>
        {
            int byScore = b.Score.CompareTo(a.Score);
            if (byScore != 0)
                return byScore;
            return b.Glue.CompareTo(a.Glue);
        });

        result = found[0];
        return true;
    }

    private bool CanOccupyFromNose(int frontX, int frontY, int dx, int dy, int size)
    {
        for (int i = 0; i < size; i++)
        {
            int x = frontX - dx * i;
            int y = frontY - dy * i;
            if (IsCellFree(x, y) == false)
                return false;
        }

        return true;
    }

    private void OccupyFromNose(int frontX, int frontY, int dx, int dy, int size)
    {
        for (int i = 0; i < size; i++)
        {
            int x = frontX - dx * i;
            int y = frontY - dy * i;
            _cells[x, y] = false;
        }

        _hasOccupiedCells = true;
    }

    private int CountGlue(int frontX, int frontY, int dx, int dy, int size)
    {
        int glue = 0;

        for (int i = 0; i < size; i++)
        {
            int x = frontX - dx * i;
            int y = frontY - dy * i;

            for (int d = 0; d < _directions.Length; d++)
            {
                int nx = x + _directions[d].x;
                int ny = y + _directions[d].y;

                if (nx < 0 || nx >= _sizeX || ny < 0 || ny >= _sizeY)
                    continue;

                if (_cells[nx, ny])
                    continue;

                if (IsOnShip(nx, ny, frontX, frontY, dx, dy, size))
                    continue;

                glue++;
            }
        }

        return glue;
    }

    private static bool IsOnShip(int x, int y, int frontX, int frontY, int dx, int dy, int size)
    {
        for (int i = 0; i < size; i++)
        {
            if (x == frontX - dx * i && y == frontY - dy * i)
                return true;
        }

        return false;
    }

    private void GetOccupiedCentroid(out float cx, out float cy, out int count)
    {
        float sumX = 0f;
        float sumY = 0f;
        count = 0;

        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
            {
                if (_cells[x, y])
                    continue;

                sumX += x;
                sumY += y;
                count++;
            }
        }

        if (count == 0)
        {
            cx = (_sizeX - 1) * 0.5f;
            cy = (_sizeY - 1) * 0.5f;
            return;
        }

        cx = sumX / count;
        cy = sumY / count;
    }

    private int BoundingBoxAreaAfter(int frontX, int frontY, int dx, int dy, int size)
    {
        GetBoundingBoxAfter(frontX, frontY, dx, dy, size, out int minX, out int minY, out int maxX, out int maxY);
        return (maxX - minX + 1) * (maxY - minY + 1);
    }

    private int CountEmptyInBoundingBoxAfter(int frontX, int frontY, int dx, int dy, int size)
    {
        GetBoundingBoxAfter(frontX, frontY, dx, dy, size, out int minX, out int minY, out int maxX, out int maxY);
        int empty = 0;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                bool onNewShip = IsOnShip(x, y, frontX, frontY, dx, dy, size);
                if (onNewShip == false && _cells[x, y])
                    empty++;
            }
        }

        return empty;
    }

    private void GetBoundingBoxAfter(int frontX, int frontY, int dx, int dy, int size, out int minX, out int minY, out int maxX, out int maxY)
    {
        minX = _sizeX;
        minY = _sizeY;
        maxX = -1;
        maxY = -1;

        for (int x = 0; x < _sizeX; x++)
        {
            for (int y = 0; y < _sizeY; y++)
            {
                bool occupied = _cells[x, y] == false || IsOnShip(x, y, frontX, frontY, dx, dy, size);
                if (occupied == false)
                    continue;

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0)
        {
            minX = 0;
            minY = 0;
            maxX = 0;
            maxY = 0;
        }
    }

    private static string FormatShipCells(int frontX, int frontY, int dx, int dy, int size)
    {
        var parts = new string[size];
        for (int i = 0; i < size; i++)
            parts[i] = $"({frontX - dx * i},{frontY - dy * i})";
        return string.Join(" -> ", parts);
    }

    private string BuildOccupancyMap(int noseX = int.MinValue, int noseY = int.MinValue, int dx = 0, int dy = 0, int size = 0)
    {
        var lines = new System.Text.StringBuilder();
        for (int y = _sizeY - 1; y >= 0; y--)
        {
            for (int x = 0; x < _sizeX; x++)
            {
                if (size > 0 && x == noseX && y == noseY)
                    lines.Append('N');
                else if (size > 0 && IsOnShip(x, y, noseX, noseY, dx, dy, size))
                    lines.Append('B');
                else if (_cells[x, y] == false)
                    lines.Append('#');
                else
                    lines.Append('.');
            }

            lines.AppendLine();
        }

        return lines.ToString();
    }

    private void LogVisualOverlaps()
    {
        Transform parent = _conteiner != null ? _conteiner : transform;
        var ships = parent.GetComponentsInChildren<ShipView>();

        for (int i = 0; i < ships.Length; i++)
        {
            Renderer a = ships[i].VisualRenderer;
            if (a == null)
                continue;

            Log.Info($"Визуал {ships[i].name} bounds={a.bounds.center} size={a.bounds.size}");

            for (int j = i + 1; j < ships.Length; j++)
            {
                Renderer b = ships[j].VisualRenderer;
                if (b == null)
                    continue;

                if (a.bounds.Intersects(b.bounds))
                    Log.Error($"Пересечение мешей: {ships[i].name} и {ships[j].name}");
            }
        }
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
        _sizeX = Mathf.Max(1, columns);
        _sizeY = Mathf.Max(1, rows);
        _cells = null;
        _cellsViewMap = null;
        EnsureInitialized();
    }

    public void SetCells(List<ParkingCell> prefabs)
    {
        _cellsView = prefabs == null ? new List<ParkingCell>() : new List<ParkingCell>(prefabs);
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

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        Transform parent = _conteiner != null ? _conteiner : transform;
        ShipView shipInstance = Instantiate(shipPrefab, frontPos, rotation, parent);
        shipInstance.SetData(shipData);
        shipInstance.transform.localScale = Vector3.one * 2.5f;

        Log.Info(
            $"  спавн noseCell=({frontX},{frontY}) dir=({facing.x},{facing.y}) forward={forward} euler={shipInstance.transform.eulerAngles}");
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