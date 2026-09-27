using System.Collections.Generic;
using UnityEngine;
using Utilites;

public class LaunchTapeGenerator
{
    private readonly ShipParkingCreator _parkingCreator;
    private readonly PuzzleGenerationSettingsSO _settings;
    private readonly int _waitSlots;
    private readonly PuzzleDifficulty _difficulty;

    public LaunchTapeGenerator(
        ShipParkingCreator parkingCreator,
        PuzzleGenerationSettingsSO settings)
    {
        _parkingCreator = parkingCreator;
        _settings = settings != null
            ? settings
            : PuzzleGenerationSettingsSO.CreateRuntimeDefault();
        _waitSlots = _settings.ResolvedWaitingSlots;
        _difficulty = _settings.Difficulty;
    }

    public List<ShipData> Build(LevelDataSO level)
    {
        EnemyBoard board = EnemyBoard.FromLevel(level);
        if (board.RemainingTotal == 0)
        {
            Log.Warn("Враг пустой — лента не из чего собирать.");
            return new List<ShipData>();
        }

        Log.Info($"Сборка ленты: блоков={board.RemainingTotal}, P={_waitSlots}, сложность={_difficulty}");

        var waiting = new List<ShipData>();
        var tape = new List<ShipData>();
        int lastColor = int.MinValue;
        int guard = board.RemainingTotal * 4 + 16;

        while (board.RemainingTotal > 0 && guard-- > 0)
        {
            if (TryFireWaiting(waiting, board))
                continue;

            if (TryLaunchFromParking(level, board, waiting, tape, ref lastColor) == false)
                break;
        }

        if (board.RemainingTotal > 0)
        {
            Log.Error($"Лента вылетов не собралась, осталось блоков: {board.RemainingTotal}");
            return null;
        }

        for (int i = 0; i < tape.Count; i++)
            tape[i].ResetAmmo();

        var summary = new System.Text.StringBuilder();
        summary.Append($"Лента готова, вылетов={tape.Count}: ");
        for (int i = 0; i < tape.Count; i++)
            summary.Append($"S{i + 1}[c{tape[i].ColorIndex}/s{tape[i].Size}/a{tape[i].MaxBullets}] ");
        Log.Info(summary.ToString());

        return tape;
    }

    private static bool TryFireWaiting(List<ShipData> waiting, EnemyBoard board)
    {
        for (int i = 0; i < waiting.Count; i++)
        {
            ShipData ship = waiting[i];
            if (board.ExposedCount(ship.ColorIndex) <= 0)
                continue;

            Fire(ship, board);

            bool stillWaiting = ship.IsAlive;
            if (stillWaiting == false)
                waiting.RemoveAt(i);

            Log.Info($"P-вылет цвет={ship.ColorIndex} патроны={ship.CurrentBullets} осталосьВP={waiting.Count}");
            return true;
        }

        return false;
    }

    private bool TryLaunchFromParking(
        LevelDataSO level,
        EnemyBoard board,
        List<ShipData> waiting,
        List<ShipData> tape,
        ref int lastColor)
    {
        List<int> colors = board.GetExposedColors();
        if (colors.Count == 0)
            return false;

        colors = OrderColors(colors, lastColor);

        for (int i = 0; i < colors.Count; i++)
        {
            if (TryCreateLaunch(level, board, waiting, tape, colors[i]) == false)
                continue;

            lastColor = colors[i];
            return true;
        }

        return false;
    }

    private bool TryCreateLaunch(
        LevelDataSO level,
        EnemyBoard board,
        List<ShipData> waiting,
        List<ShipData> tape,
        int colorIndex)
    {
        int exposed = board.ExposedCount(colorIndex);
        int remaining = board.RemainingCount(colorIndex);
        if (exposed <= 0 || remaining <= 0)
            return false;

        if (_parkingCreator.TryPickShip(remaining, exposed, waiting.Count, _waitSlots, _difficulty, out int size, out int ammo) == false)
            return false;

        Color color = level.GetColor(colorIndex);
        var ship = new ShipData(size, ammo, ammo, color, colorIndex);

        int shot = Mathf.Min(ammo, exposed);
        if (ammo > shot && waiting.Count >= _waitSlots)
            return false;

        Fire(ship, board);

        if (ship.IsAlive)
        {
            if (waiting.Count >= _waitSlots)
                return false;

            waiting.Add(ship);
        }

        tape.Add(ship);
        Log.Info($"S{tape.Count}: цвет={colorIndex} size={size} ammo={ammo} снято={shot} вP={ship.IsAlive} открытоБыло={exposed} остатокЦвета={board.RemainingCount(colorIndex)}");
        return true;
    }

    private List<int> OrderColors(List<int> colors, int lastColor)
    {
        var ordered = new List<int>(colors);
        ordered.Shuffle();

        if (lastColor == int.MinValue || ordered.Count <= 1)
            return ordered;

        int preferred = 0;
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i] != lastColor)
            {
                preferred = i;
                break;
            }
        }

        if (preferred != 0)
            (ordered[0], ordered[preferred]) = (ordered[preferred], ordered[0]);

        return ordered;
    }

    private static void Fire(ShipData ship, EnemyBoard board)
    {
        int open = board.ExposedCount(ship.ColorIndex);
        int shot = Mathf.Min(ship.CurrentBullets, open);
        board.RemoveExposed(ship.ColorIndex, shot);
        ship.Spend(shot);
    }
}
