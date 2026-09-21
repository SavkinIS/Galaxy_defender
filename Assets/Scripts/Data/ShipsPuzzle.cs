using System.Collections.Generic;
using UnityEngine;

public class ShipsPuzzle : MonoBehaviour
{
    [SerializeField] private ShipView _shipViewS1;
    [SerializeField] private ShipView _shipViewS2;
    [SerializeField] private ShipView _shipViewS3;
    [SerializeField] private ShipParkingCreator _parkingCreator;
    [SerializeField] private ParkingGrid _parkingGrid;
    [SerializeField] private LevelDataSO _levelData;
    [SerializeField] private PuzzleGenerationSettingsSO _generationSettings;
    [SerializeField] private bool _debugLogs = true;

    private Dictionary<int, ShipView> _shipsById;
    private List<ShipData> _launchTape;
    private int _nextIndex;

    private void Start()
    {
        ParkingLog.Enabled = _debugLogs;
        _parkingCreator.Configure(_generationSettings);

        _shipsById = new Dictionary<int, ShipView>
        {
            { _parkingCreator.SmallShipSize, _shipViewS1 },
            { _parkingCreator.MediumShipSize, _shipViewS2 },
            { _parkingCreator.LargeShipSize, _shipViewS3 },
        };

        var solver = new GuaranteedLaunchTapeSolver(
            _parkingCreator,
            _parkingGrid,
            _generationSettings);
        _launchTape = solver.Build(_levelData);

        if (_launchTape == null || _launchTape.Count == 0)
        {
            ParkingLog.Warn("Гарантированная лента штурмовиков не создана.");
            _nextIndex = -1;
            return;
        }

        if (_parkingGrid.TryPlaceTapeReverse(_launchTape, GetPrefabBySize) == false)
        {
            ParkingLog.Error("Solver вернул ленту, но визуальная укладка не прошла.");
            _launchTape = null;
            _nextIndex = -1;
            return;
        }

        _nextIndex = _launchTape.Count - 1;
        ParkingLog.Info($"Гарантированная лента размещена. Кораблей={_launchTape.Count}, слотов={solver.WaitSlots}.");
    }

    private ShipView GetPrefabBySize(int size)
    {
        return _shipsById.TryGetValue(size, out ShipView prefab) ? prefab : null;
    }

    private void PlaceNextShip()
    {
        if (_launchTape == null || _nextIndex < 0)
        {
            ParkingLog.Warn("Больше кораблей нет.");
            return;
        }

        ShipData data = _launchTape[_nextIndex];
        if (_shipsById.TryGetValue(data.Size, out ShipView prefab) == false || prefab == null)
        {
            ParkingLog.Error($"Нет префаба для size={data.Size}");
            _nextIndex--;
            return;
        }

        ParkingLog.Info($"Клик: кладу S{_nextIndex + 1}/{_launchTape.Count} size={data.Size} цвет={data.ColorIndex}");

        if (_parkingGrid.TryPlaceOne(data, prefab, _nextIndex) == false)
            ParkingLog.Error($"Не удалось поставить S{_nextIndex + 1}");

        _nextIndex--;
        ParkingLog.Info(_nextIndex >= 0 ? $"Осталось кликов: {_nextIndex + 1}" : "Лента закончилась.");
    }
}
