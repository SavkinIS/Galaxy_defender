using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class ShipGenerationRegressionChecks
{
    private const string MapPath = "Assets/Data/MapData/map_1_LevelData 2.asset";

    [MenuItem("Tools/Generation/Run Regression Checks")]
    public static void Run()
    {
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Run in Editor EditMode only.");
        bool logging = Log.Enabled;
        bool unityLogging = Debug.unityLogger.logEnabled;
        var summary = new List<string>();
        try
        {
            Log.Enabled = false;
            Debug.unityLogger.logEnabled = false;
            CheckBoards();
            LevelDataSO level = AssetDatabase.LoadAssetAtPath<LevelDataSO>(MapPath);
            Require(level != null, "Map not found: " + MapPath);
            for (int value = 1; value <= 5; value++)
                summary.Add(CheckGeneration(level, (PuzzleDifficulty)value));
            summary.Add(CheckGeneration(level, PuzzleDifficulty.Hardcore, 2));
        }
        finally
        {
            Log.Enabled = logging;
            Debug.unityLogger.logEnabled = unityLogging;
        }
        // Print the explicit result even when logging was disabled before this run.
        try
        {
            Debug.unityLogger.logEnabled = true;
            Debug.Log("Generation regression checks OK; boards OK; " + string.Join("; ", summary));
        }
        finally { Debug.unityLogger.logEnabled = unityLogging; }
    }

    private static void CheckBoards()
    {
        LevelDataSO level = ScriptableObject.CreateInstance<LevelDataSO>();
        try
        {
            var cells = new LevelDataSO.CellData[9];
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    cells[y * 3 + x].Set(x, y, x == 1 && y == 1 ? 1 : 0);
            level.SetGridData(3, 3, cells);
            EnemyBoard board = EnemyBoard.FromLevel(level);
            Require(!board.IsExposed(1, 1, 0) && board.ExposedCount(0) == 8 &&
                board.ExposedCount(1) == 0, "Ring must hide its center.");
            Require(board.RemoveExposed(1, 10) == 0, "Shot passed through the ring.");
            EnemyBoard clone = board.Clone();
            Require(clone.GetStateKey() == board.GetStateKey(), "Clone changed initial state.");
            Require(clone.RemoveExposed(0, 10) == 8 && clone.IsExposed(1, 1, 0) &&
                clone.ExposedCount(1) == 1, "Removing ring must expose center.");
            Require(board.RemainingTotal == 9 && board.ExposedCount(0) == 8 &&
                !board.IsExposed(1, 1, 0), "Clone mutation affected original.");
            Require(clone.RemoveExposed(1, 10) == 1 && clone.RemainingTotal == 0,
                "Clone center was not removed.");
            Require(board.RemoveExposed(0, 10) == 8 && board.RemoveExposed(1, 10) == 1 &&
                board.RemainingTotal == 0, "Original ring replay failed.");

            cells = new LevelDataSO.CellData[49];
            for (int y = 0; y < 7; y++)
                for (int x = 0; x < 7; x++)
                {
                    int depth = Mathf.Min(Mathf.Min(x, 6 - x), Mathf.Min(y, 6 - y));
                    cells[y * 7 + x].Set(x, y, depth == 2 ? 1 : 0);
                }
            level.SetGridData(7, 7, cells);
            board = EnemyBoard.FromLevel(level);
            Require(board.ExposedCount(0) == 24, "Unexpected outer perimeter.");
            // Forty targets exceed the initial perimeter: firing must revisit newly opened lines.
            Require(board.RemoveExposed(0, 100) == 40 && board.RemainingCount(0) == 1 &&
                board.RemainingCount(1) == 8, "Continuous layers or color barrier failed.");
            Require(board.RemoveExposed(0, 100) == 0 && !board.IsExposed(3, 3, 0),
                "Firing crossed another color.");
            Require(board.RemoveExposed(1, 100) == 8 && board.RemoveExposed(0, 100) == 1 &&
                board.RemainingTotal == 0, "Layered board did not clear.");
        }
        finally { Object.DestroyImmediate(level); }
    }

    private static string CheckGeneration(LevelDataSO level, PuzzleDifficulty difficulty, int availableSlots = 6)
    {
        var clock = Stopwatch.StartNew();
        PuzzleGenerationSettingsSO settings = null;
        GameObject creatorObject = null, parkingObject = null;
        try
        {
            settings = PuzzleGenerationSettingsSO.CreateRuntimeDefault();
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("_difficulty").intValue = (int)difficulty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            creatorObject = new GameObject("Regression creator") { hideFlags = HideFlags.HideAndDontSave };
            creatorObject.SetActive(false); // Avoid Awake creating an unowned runtime settings object.
            var creator = creatorObject.AddComponent<ShipParkingCreator>();
            creator.Configure(settings);
            parkingObject = new GameObject("Regression parking") { hideFlags = HideFlags.HideAndDontSave };
            parkingObject.SetActive(false);
            var parking = parkingObject.AddComponent<ParkingGrid>();
            parking.SetSize(10, 10);
            var solver = new GuaranteedLaunchTapeSolver(creator, parking, settings, availableSlots);
            Require(solver.Difficulty == difficulty && solver.WaitSlots == Mathf.Min(availableSlots, settings.ResolvedWaitingSlots),
                "Difficulty or waiting limit mismatch.");
            List<ShipData> tape = solver.Build(level);
            Require(tape != null && tape.Count > 0, "Build returned no tape.");
            EnemyBoard board = EnemyBoard.FromLevel(level);
            var ammo = new Dictionary<int, int>();
            foreach (ShipData ship in tape)
            {
                Require(ship != null && ship.MaxBullets > 0, "Invalid ship.");
                Require(ship.Size == creator.FindShipSize(ship.MaxBullets) &&
                    ship.CurrentBullets == ship.MaxBullets, "Size or initial ammo mismatch.");
                Require(ship.MaxBullets <= creator.GetAmmoForSize(ship.Size), "Hull capacity exceeded.");
                ammo.TryGetValue(ship.ColorIndex, out int total);
                ammo[ship.ColorIndex] = total + ship.MaxBullets;
            }
            int totalAmmo = 0;
            foreach (var entry in ammo)
            {
                Require(entry.Value == board.RemainingCount(entry.Key), "Ammo mismatch for color " + entry.Key);
                totalAmmo += entry.Value;
            }
            Require(totalAmmo == board.RemainingTotal, "Tape omits board colors.");
            int peak = Replay(tape, board, solver.WaitSlots);
            Require(parking.CanPlaceTapeReverse(tape), "Reverse parking rejected tape.");
            return $"{difficulty}: ships={tape.Count}, waiting={peak}/{solver.WaitSlots}, {clock.ElapsedMilliseconds} ms";
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Generation regression [{difficulty}] after {clock.ElapsedMilliseconds} ms", exception);
        }
        finally
        {
            if (creatorObject != null) Object.DestroyImmediate(creatorObject);
            if (parkingObject != null) Object.DestroyImmediate(parkingObject);
            if (settings != null) Object.DestroyImmediate(settings);
        }
    }

    private static int Replay(IReadOnlyList<ShipData> tape, EnemyBoard board, int waitSlots)
    {
        var waiting = new List<ShipData>();
        int peak = 0;
        foreach (ShipData source in tape)
        {
            Stabilize(waiting, board);
            Require(board.ExposedCount(source.ColorIndex) > 0, "Launch color is not available.");
            var ship = new ShipData(source.Size, source.MaxBullets, source.MaxBullets, source.Color, source.ColorIndex);
            ship.Spend(board.RemoveExposed(ship.ColorIndex, ship.CurrentBullets));
            if (ship.HasBullets) waiting.Add(ship);
            peak = Mathf.Max(peak, waiting.Count);
            Require(peak <= waitSlots, "Waiting limit exceeded.");
            Stabilize(waiting, board);
        }
        Stabilize(waiting, board);
        Require(board.RemainingTotal == 0 && waiting.Count == 0, "Replay left blocks or waiting ships.");
        return peak;
    }

    private static void Stabilize(List<ShipData> waiting, EnemyBoard board)
    {
        int before;
        do
        {
            before = board.RemainingTotal;
            for (int index = 0; index < waiting.Count;)
            {
                ShipData ship = waiting[index];
                ship.Spend(board.RemoveExposed(ship.ColorIndex, ship.CurrentBullets));
                if (!ship.HasBullets) waiting.RemoveAt(index);
                else index++;
            }
        } while (board.RemainingTotal < before);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
