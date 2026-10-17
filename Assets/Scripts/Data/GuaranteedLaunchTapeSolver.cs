using System.Collections.Generic;
using UnityEngine;

public sealed class GuaranteedLaunchTapeSolver
{
    private const int NoRemainingBlocks = 0;
    private const int MaxVisitedStates = 5000;

    private readonly ShipParkingCreator _parkingCreator;
    private readonly ParkingGrid _parkingGrid;
    private readonly PuzzleGenerationSettingsSO _settings;
    private readonly PuzzleDifficulty _difficulty;
    private readonly int _waitSlots;
    private readonly IReadOnlyList<PathPoint> _battlePoints;
    private readonly EnemyCellConnector _connector;
    private readonly HashSet<string> _failedStates = new HashSet<string>();
    private int _visitedStates;

    public PuzzleDifficulty Difficulty => _difficulty;
    public int WaitSlots => _waitSlots;

    public GuaranteedLaunchTapeSolver(
        ShipParkingCreator parkingCreator,
        ParkingGrid parkingGrid,
        PuzzleGenerationSettingsSO settings,
        int availableWaitingSlots = -1,
        IReadOnlyList<PathPoint> battlePoints = null,
        EnemyCellConnector connector = null)
    {
        _parkingCreator = parkingCreator;
        _parkingGrid = parkingGrid;
        _settings = settings != null
            ? settings
            : PuzzleGenerationSettingsSO.CreateRuntimeDefault();
        _difficulty = _settings.Difficulty;
        _waitSlots = availableWaitingSlots < 0
            ? _settings.ResolvedWaitingSlots
            : Mathf.Min(availableWaitingSlots, _settings.ResolvedWaitingSlots);
        _battlePoints = battlePoints;
        _connector = connector;
    }

    public List<ShipData> Build(LevelDataSO level)
    {
        EnemyBoard board = EnemyBoard.FromLevel(level, _battlePoints, _connector);
        if (board.RemainingTotal == NoRemainingBlocks)
            return new List<ShipData>();

        _failedStates.Clear();
        _visitedStates = 0;

        var tape = new List<ShipData>();
        var waiting = new List<ShipData>();
        if (Search(level, board, waiting, tape))
        {
            for (int i = 0; i < tape.Count; i++)
                tape[i].ResetAmmo();

            Log.Info($"Гарантированная лента готова: кораблей={tape.Count}, слотов={_waitSlots}, посещено состояний={_visitedStates}");
            return tape;
        }

        Log.Error($"Не найдено гарантированное решение. Посещено состояний={_visitedStates}");
        return null;
    }

    private bool Search(
        LevelDataSO level,
        EnemyBoard board,
        List<ShipData> waiting,
        List<ShipData> tape)
    {
        if (_visitedStates >= MaxVisitedStates || !HasParkingCapacity(board, waiting, tape))
            return false;

        _visitedStates++;
        if (board.RemainingTotal == 0)
        {
            for (int i = 0; i < waiting.Count; i++)
            {
                if (waiting[i].HasBullets)
                    return false;
            }

            return _parkingGrid == null || _parkingGrid.CanPlaceTapeReverse(tape);
        }

        string stateKey = BuildStateKey(board, waiting, tape);
        if (_failedStates.Contains(stateKey))
            return false;

        for (int i = 0; i < waiting.Count; i++)
        {
            ShipData waitingShip = waiting[i];
            if (board.ExposedCount(waitingShip.ColorIndex) <= 0)
                continue;

            EnemyBoard nextBoard = board.Clone();
            ShipData nextShip = CloneShip(waitingShip);
            Fire(nextShip, nextBoard);

            var nextWaiting = CloneShips(waiting);
            nextWaiting[i] = nextShip;
            if (nextShip.HasBullets == false)
                nextWaiting.RemoveAt(i);

            if (Search(level, nextBoard, nextWaiting, tape))
                return true;
        }

        List<int> colors = OrderColors(board.GetExposedColors(), board, waiting.Count);
        for (int colorIndex = 0; colorIndex < colors.Count; colorIndex++)
        {
            int color = colors[colorIndex];
            int remaining = board.RemainingCount(color);
            for (int index = 0; index < waiting.Count; index++)
            {
                if (waiting[index].ColorIndex == color)
                    remaining -= waiting[index].CurrentBullets;
            }
            if (remaining <= 0)
                continue;

            var triedAmmo = new HashSet<int>();
            foreach (int size in _parkingCreator.GetSizePriority(_difficulty))
            {
                int ammo = Mathf.Min(_parkingCreator.GetAmmoForSize(size), remaining);
                if (triedAmmo.Add(ammo) && TryLaunch(level, board, waiting, tape, color, ammo))
                    return true;
            }

            // При заполненном ожидании допускается неполный корабль, который достреляет сразу.
            int exposedAmmo = Mathf.Min(board.ExposedCount(color), remaining);
            if (triedAmmo.Add(exposedAmmo) &&
                TryLaunch(level, board, waiting, tape, color, exposedAmmo))
                return true;
        }

        _failedStates.Add(stateKey);
        return false;
    }

    private List<int> OrderColors(List<int> colors, EnemyBoard board, int waitingCount)
    {
        bool prioritizeProgress = IsLowDifficulty(_difficulty) || waitingCount >= Mathf.Max(0, _waitSlots - 1);
        colors.Sort((left, right) =>
        {
            int leftCount = board.ExposedCount(left);
            int rightCount = board.ExposedCount(right);
            int comparison = prioritizeProgress
                ? rightCount.CompareTo(leftCount)
                : leftCount.CompareTo(rightCount);
            return comparison != 0 ? comparison : left.CompareTo(right);
        });

        return colors;
    }

    private static bool IsLowDifficulty(PuzzleDifficulty difficulty)
    {
        return difficulty == PuzzleDifficulty.Training || difficulty == PuzzleDifficulty.Easy;
    }

    private static void Fire(ShipData ship, EnemyBoard board)
    {
        int shot = board.RemoveExposed(ship.ColorIndex, ship.CurrentBullets);
        ship.Spend(shot);
    }

    private bool TryLaunch(LevelDataSO level, EnemyBoard board, List<ShipData> waiting,
        List<ShipData> tape, int color, int ammo)
    {
        if (ammo <= 0)
            return false;

        int size = _parkingCreator.FindShipSize(ammo);
        if (ammo > _parkingCreator.GetAmmoForSize(size))
            return false;

        var nextShip = new ShipData(size, ammo, ammo, level.GetColor(color), color);
        EnemyBoard nextBoard = board.Clone();
        Fire(nextShip, nextBoard);
        if (nextShip.HasBullets && waiting.Count >= _waitSlots)
            return false;

        var nextWaiting = CloneShips(waiting);
        if (nextShip.HasBullets)
            nextWaiting.Add(nextShip);

        tape.Add(nextShip);
        if (Search(level, nextBoard, nextWaiting, tape))
            return true;

        tape.RemoveAt(tape.Count - 1);
        return false;
    }

    private bool HasParkingCapacity(EnemyBoard board, List<ShipData> waiting, List<ShipData> tape)
    {
        if (_parkingGrid == null)
            return true;

        int occupied = 0;
        for (int index = 0; index < tape.Count; index++)
            occupied += tape[index].Size;

        int unassigned = board.RemainingTotal;
        for (int index = 0; index < waiting.Count; index++)
            unassigned -= waiting[index].CurrentBullets;

        int bestAmmo = 0;
        int bestSize = 1;
        foreach (int size in _parkingCreator.GetSizePriority(_difficulty))
        {
            int ammo = _parkingCreator.GetAmmoForSize(size);
            if ((long)ammo * bestSize > (long)bestAmmo * size)
            {
                bestAmmo = ammo;
                bestSize = size;
            }
        }

        long requiredCells = ((long)Mathf.Max(0, unassigned) * bestSize + bestAmmo - 1) / bestAmmo;
        return occupied + requiredCells <= _parkingGrid.Capacity;
    }

    private static ShipData CloneShip(ShipData source)
    {
        return new ShipData(source.Size, source.MaxBullets, source.CurrentBullets, source.Color, source.ColorIndex);
    }

    private static List<ShipData> CloneShips(List<ShipData> ships)
    {
        var clone = new List<ShipData>(ships.Count);
        for (int i = 0; i < ships.Count; i++)
            clone.Add(CloneShip(ships[i]));
        return clone;
    }

    private static string BuildStateKey(EnemyBoard board, List<ShipData> waiting, List<ShipData> tape)
    {
        var key = new System.Text.StringBuilder(board.GetStateKey());
        key.Append("|W:");
        for (int i = 0; i < waiting.Count; i++)
            key.Append(waiting[i].ColorIndex).Append(':').Append(waiting[i].CurrentBullets).Append(';');
        key.Append("|T:");
        for (int i = 0; i < tape.Count; i++)
            key.Append(tape[i].ColorIndex).Append(':').Append(tape[i].Size).Append(';');
        return key.ToString();
    }
}
