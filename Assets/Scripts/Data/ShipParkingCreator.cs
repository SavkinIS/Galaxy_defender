using System.Collections.Generic;
using UnityEngine;

public class ShipParkingCreator : MonoBehaviour
{
    [SerializeField] private int _shipSize1 = 1;
    [SerializeField] private int _shipSize2 = 2;
    [SerializeField] private int _shipSize3 = 3;
    
    private Dictionary<int, int> _bulletsBySize;

    private void Awake()
    {
        EnsureAmmoTable();
    }

    private void EnsureAmmoTable()
    {
        if (_bulletsBySize != null)
            return;

        _bulletsBySize = new Dictionary<int, int>
        {
            { _shipSize1, 10 },
            { _shipSize2, 20 },
            { _shipSize3, 30 },
        };
    }

    public bool TryPickShip(
        int remainingOfColor,
        int exposedOfColor,
        int waitingCount,
        int waitSlots,
        int difficulty,
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
            int candidateAmmo = Mathf.Min(_bulletsBySize[candidateSize], remainingOfColor);
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

    public List<int> GetSizePriority(int difficulty)
    {
        switch (difficulty)
        {
            case 3:
                return new List<int> { _shipSize2, _shipSize3, _shipSize1 };
            case 4:
            case 5:
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

    public List<ShipData> GenerateShipsForColor(int blocksCount, int difficulty, Color color, int colorIndex)
    {
        EnsureAmmoTable();
        Queue<int> sizes = new Queue<int>();

        switch (difficulty)
        {
            case 1:
            case 2:
                sizes.Enqueue(_shipSize3);
                sizes.Enqueue(_shipSize2);
                sizes.Enqueue(_shipSize1);
                break;
            case 3:
                sizes.Enqueue(_shipSize2);
                sizes.Enqueue(_shipSize3);
                sizes.Enqueue(_shipSize1);
                break;
            case 4:
            case 5:
                sizes.Enqueue(_shipSize1);
                sizes.Enqueue(_shipSize2);
                sizes.Enqueue(_shipSize3);
                break;
            default:
                sizes.Enqueue(_shipSize3);
                sizes.Enqueue(_shipSize2);
                sizes.Enqueue(_shipSize1);
                break;
        }
        
        int currentBlocks = blocksCount;
        
        List<ShipData> datas = new List<ShipData>();

        int currentSize = sizes.Dequeue();

        while (currentBlocks > 0)
        {  
            int bullets = _bulletsBySize[currentSize];
            int size = currentSize;

            if (currentBlocks >= bullets)
            {
                currentBlocks -= bullets;
                
                if (currentBlocks < bullets)
                {
                    if (sizes.Count > 0)
                        currentSize = sizes.Dequeue();
                }
            }
            else
            {
                bullets = currentBlocks;
                currentBlocks -= bullets;

                size = FindShipSize(bullets);
            }
            
            ShipData data = new  ShipData(size, bullets, bullets, color, colorIndex);
            datas.Add(data);
        }
        
        return datas;
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
}