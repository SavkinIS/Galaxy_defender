using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipParkingCreator : MonoBehaviour
{
    [SerializeField] private int _shipSize1 = 1;
    [SerializeField] private int _shipSize2 = 2;
    [SerializeField] private int _shipSize3 = 3;
    
    private Dictionary<int, int> _bulletsBySize;

    private void Awake()
    {
        _bulletsBySize = new Dictionary<int, int>()
        {
            { _shipSize1, 10 },
            { _shipSize2, 20 },
            { _shipSize3, 30 },
        };
    }

    public List<ShipData> GenerateShipsForColor(int blocksCount, int difficulty, Color color, int colorIndex)
    {
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