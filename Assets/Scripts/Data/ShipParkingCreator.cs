using System.Collections.Generic;
using UnityEngine;

public class ShipParkingCreator : MonoBehaviour
{
    private const int DefaultSmallShipSize = 1;
    private const int DefaultMediumShipSize = 2;
    private const int DefaultLargeShipSize = 3;
    private const int SmallShipAmmo = 10;
    private const int MediumShipAmmo = 20;
    private const int LargeShipAmmo = 30;

    [SerializeField] private int _shipSize1 = DefaultSmallShipSize;
    [SerializeField] private int _shipSize2 = DefaultMediumShipSize;
    [SerializeField] private int _shipSize3 = DefaultLargeShipSize;

    private Dictionary<int, int> _bulletsBySize;

    private void Awake()
    {
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
        size = _shipSize1;
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
                return new List<int> { _shipSize2, _shipSize3, _shipSize1 };
            case PuzzleDifficulty.Hard:
            case PuzzleDifficulty.Hardcore:
                return new List<int> { _shipSize1, _shipSize2, _shipSize3 };
            default:
                return new List<int> { _shipSize3, _shipSize2, _shipSize1 };
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

        return _shipSize3;
    }

    private void EnsureAmmoTable()
    {
        if (_bulletsBySize != null)
            return;

        _bulletsBySize = new Dictionary<int, int>
        {
            { _shipSize1, SmallShipAmmo },
            { _shipSize2, MediumShipAmmo },
            { _shipSize3, LargeShipAmmo },
        };
    }
}