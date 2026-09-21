using System.Collections.Generic;
using UnityEngine;

public class ShipParkingCreator : MonoBehaviour
{
    private PuzzleGenerationSettingsSO _generationSettings;
    private PuzzleGenerationSettingsSO _runtimeSettings;
    private Dictionary<int, int> _bulletsBySize;

    public int SmallShipSize => GetSettings().SmallShipSize;
    public int MediumShipSize => GetSettings().MediumShipSize;
    public int LargeShipSize => GetSettings().LargeShipSize;

    private void Awake()
    {
        EnsureAmmoTable();
    }

    public void Configure(PuzzleGenerationSettingsSO settings)
    {
        _generationSettings = settings;
        _bulletsBySize = null;
        EnsureAmmoTable();
    }

    public bool TryPickShip(
        int remainingOfColor,
        int exposedOfColor,
        int waitingCount,
        int waitSlots,
        PuzzleDifficulty difficulty,
        out int size,
        out int ammo)
    {
        EnsureAmmoTable();
        size = SmallShipSize;
        ammo = 0;

        if (remainingOfColor <= 0 || exposedOfColor <= 0)
            return false;

        List<int> sizes = GetSizePriority(difficulty);
        bool waitFull = waitingCount >= waitSlots;

        for (int i = 0; i < sizes.Count; i++)
        {
            int candidateSize = sizes[i];
            int candidateAmmo = Mathf.Min(GetAmmoForSize(candidateSize), remainingOfColor);
            if (candidateAmmo <= 0)
                continue;

            bool willWait = candidateAmmo > exposedOfColor;
            if (willWait && waitFull)
                continue;

            size = FindShipSize(candidateAmmo);
            ammo = candidateAmmo;
            return true;
        }

        ammo = Mathf.Min(exposedOfColor, remainingOfColor);
        if (ammo <= 0)
            return false;

        size = FindShipSize(ammo);
        return true;
    }

    public List<int> GetSizePriority(PuzzleDifficulty difficulty)
    {
        switch (difficulty)
        {
            case PuzzleDifficulty.Medium:
                return new List<int> { MediumShipSize, LargeShipSize, SmallShipSize };
            case PuzzleDifficulty.Hard:
            case PuzzleDifficulty.Hardcore:
                return new List<int> { SmallShipSize, MediumShipSize, LargeShipSize };
            default:
                return new List<int> { LargeShipSize, MediumShipSize, SmallShipSize };
        }
    }

    public int GetAmmoForSize(int size)
    {
        EnsureAmmoTable();
        return _bulletsBySize.TryGetValue(size, out int ammo) ? ammo : 0;
    }

    public List<ShipData> GenerateShipsForColor(int blocksCount, PuzzleDifficulty difficulty, Color color, int colorIndex)
    {
        EnsureAmmoTable();

        List<int> sizePriority = GetSizePriority(difficulty);
        var ships = new List<ShipData>();
        int remainingBlocks = blocksCount;

        while (remainingBlocks > 0)
        {
            int size = FindPreferredFittingSize(sizePriority, remainingBlocks);
            int capacity = GetAmmoForSize(size);
            if (capacity <= 0)
                break;

            int ammo = Mathf.Min(capacity, remainingBlocks);
            if (ammo < capacity)
                size = FindShipSize(ammo);

            ships.Add(new ShipData(size, ammo, ammo, color, colorIndex));
            remainingBlocks -= ammo;
        }

        return ships;
    }

    private int FindPreferredFittingSize(List<int> sizePriority, int remainingBlocks)
    {
        for (int i = 0; i < sizePriority.Count; i++)
        {
            int size = sizePriority[i];
            if (GetAmmoForSize(size) <= remainingBlocks)
                return size;
        }

        return sizePriority[sizePriority.Count - 1];
    }

    private int FindShipSize(int bullets)
    {
        foreach (var size in _bulletsBySize)
        {
            if (size.Value >= bullets)
                return size.Key;
        }

        return LargeShipSize;
    }

    private PuzzleGenerationSettingsSO GetSettings()
    {
        if (_generationSettings != null)
            return _generationSettings;

        if (_runtimeSettings == null)
            _runtimeSettings = PuzzleGenerationSettingsSO.CreateRuntimeDefault();

        return _runtimeSettings;
    }

    private void EnsureAmmoTable()
    {
        if (_bulletsBySize != null)
            return;

        PuzzleGenerationSettingsSO settings = GetSettings();
        _bulletsBySize = new Dictionary<int, int>
        {
            { settings.SmallShipSize, settings.SmallShipAmmo },
            { settings.MediumShipSize, settings.MediumShipAmmo },
            { settings.LargeShipSize, settings.LargeShipAmmo },
        };
    }
}