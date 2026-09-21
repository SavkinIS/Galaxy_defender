using System.Collections.Generic;
using UnityEngine;

public sealed class GuaranteedLaunchTapeSolver
{
    private const int SingleLayerCount = 1;
    private const int MinColorAttempts = 1;
    private const int NoRemainingBlocks = 0;

    private readonly ShipParkingCreator _parkingCreator;
    private readonly ParkingGrid _parkingGrid;
    private readonly PuzzleGenerationSettingsSO _settings;
    private readonly PuzzleDifficulty _difficulty;
    private readonly int _waitSlots;
    private readonly HashSet<string> _failedStates = new HashSet<string>();
    private int _visitedStates;

    public PuzzleDifficulty Difficulty => _difficulty;
    public int WaitSlots => _waitSlots;

    public GuaranteedLaunchTapeSolver(
        ShipParkingCreator parkingCreator,
        ParkingGrid parkingGrid,
        PuzzleGenerationSettingsSO settings)
    {
        _parkingCreator = parkingCreator;
        _parkingGrid = parkingGrid;
        _settings = settings != null
            ? settings
            : PuzzleGenerationSettingsSO.CreateRuntimeDefault();
        _difficulty = _settings.Difficulty;
        _waitSlots = _settings.ResolvedWaitingSlots;
    }

    public List<ShipData> Build(LevelDataSO level)
    {
        EnemyBoard board = EnemyBoard.FromLevel(level);
        if (board.RemainingTotal == NoRemainingBlocks)
            return new List<ShipData>();

        if (board.Layers == SingleLayerCount)
            return BuildSingleLayer(level, board);

        _failedStates.Clear();
        _visitedStates = 0;

        var tape = new List<ShipData>();
        var waiting = new List<ShipData>();
        if (Search(level, board, waiting, tape))
        {
            for (int i = 0; i < tape.Count; i++)
                tape[i].ResetAmmo();

            ParkingLog.Info($"Гарантированная лента готова: кораблей={tape.Count}, слотов={_waitSlots}, посещено состояний={_visitedStates}");
            return tape;
        }

        ParkingLog.Error($"Не найдено гарантированное решение. Посещено состояний={_visitedStates}");
        return null;
    }

    private List<ShipData> BuildSingleLayer(LevelDataSO level, EnemyBoard board)
    {
        List<int> colors = OrderColors(board.GetExposedColors(), board);
        List<int> sizes = _parkingCreator.GetSizePriority(_difficulty);

        int attemptCount = Mathf.Max(MinColorAttempts, colors.Count);
        
        for (int shift = 0; shift < attemptCount; shift++)
        {
            var orderedColors = new List<int>(colors.Count);
            for (int i = 0; i < colors.Count; i++)
                orderedColors.Add(colors[(i + shift) % colors.Count]);

            var tape = new List<ShipData>();
            for (int i = 0; i < orderedColors.Count; i++)
            {
                int color = orderedColors[i];
                int remaining = board.RemainingCount(color);

                while (remaining > 0)
                {
                    int size = sizes[0];
                    for (int sizeIndex = 0; sizeIndex < sizes.Count; sizeIndex++)
                    {
                        int candidate = sizes[sizeIndex];
                        if (_parkingCreator.GetAmmoForSize(candidate) > 0)
                        {
                            size = candidate;
                            break;
                        }
                    }

                    int ammo = Mathf.Min(_parkingCreator.GetAmmoForSize(size), remaining);
                    if (ammo <= 0)
                        break;

                    tape.Add(new ShipData(size, ammo, ammo, level.GetColor(color), color));
                    remaining -= ammo;
                }
            }

            if (_parkingGrid == null || _parkingGrid.CanPlaceTapeReverse(tape))
            {
                for (int i = 0; i < tape.Count; i++)
                    tape[i].ResetAmmo();

                ParkingLog.Info($"Однослойная лента готова: кораблей={tape.Count}, слотов={_waitSlots}, попытка={shift + MinColorAttempts}");
                return tape;
            }
        }

        ParkingLog.Error("Однослойную ленту не удалось разместить на парковке.");
        return null;
    }

    private bool Search(
        LevelDataSO level,
        EnemyBoard board,
        List<ShipData> waiting,
        List<ShipData> tape)
    {
        _visitedStates++;
        if (board.RemainingTotal == 0)
        {
            for (int i = 0; i < waiting.Count; i++)
            {
                if (waiting[i].IsAlive)
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
            if (nextShip.IsAlive == false)
                nextWaiting.RemoveAt(i);

            if (Search(level, nextBoard, nextWaiting, new List<ShipData>(tape)))
                return true;
        }

        List<int> colors = OrderColors(board.GetExposedColors(), board);
        for (int colorIndex = 0; colorIndex < colors.Count; colorIndex++)
        {
            int color = colors[colorIndex];
            int remaining = board.RemainingCount(color);
            int exposed = board.ExposedCount(color);
            List<int> sizes = _parkingCreator.GetSizePriority(_difficulty);

            for (int sizeIndex = 0; sizeIndex < sizes.Count; sizeIndex++)
            {
                int size = sizes[sizeIndex];
                int ammo = Mathf.Min(_parkingCreator.GetAmmoForSize(size), remaining);
                if (ammo <= 0 || (ammo > exposed && waiting.Count >= _waitSlots))
                    continue;

                EnemyBoard nextBoard = board.Clone();
                ShipData nextShip = new ShipData(size, ammo, ammo, level.GetColor(color), color);
                Fire(nextShip, nextBoard);

                var nextWaiting = CloneShips(waiting);
                if (nextShip.IsAlive)
                    nextWaiting.Add(nextShip);

                if (nextWaiting.Count > _waitSlots)
                    continue;

                var nextTape = new List<ShipData>(tape) { nextShip };
                if (Search(level, nextBoard, nextWaiting, nextTape))
                    return true;
            }
        }

        _failedStates.Add(stateKey);
        return false;
    }

    private List<int> OrderColors(List<int> colors, EnemyBoard board)
    {
        colors.Sort((left, right) =>
        {
            int leftCount = board.ExposedCount(left);
            int rightCount = board.ExposedCount(right);
            int comparison = IsLowDifficulty(_difficulty)
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
        int shot = Mathf.Min(ship.CurrentBullets, board.ExposedCount(ship.ColorIndex));
        board.RemoveExposed(ship.ColorIndex, shot);
        ship.Spend(shot);
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
