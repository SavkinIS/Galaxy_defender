using System;
using UnityEngine;

[Serializable]
public class ShipData
{
    public event Action Cleared;
    
    public ShipData (int size, int maxBullets, int currentBullets, Color color, int colorIndex)
    {
        Size = size;
        MaxBullets = maxBullets;
        CurrentBullets = currentBullets;
        Color = color;
        ColorIndex = colorIndex;
    }

    public bool IsAlive => CurrentBullets > 0;
    public int Size { get;}
    public int MaxBullets { get;}
    public int ColorIndex{ get; }
    public Color Color { get; }
    public int CurrentBullets { get; private set;}
    
    public void DecreaseBullets()
    {
        if (CurrentBullets > 0)
            CurrentBullets--;
        
        if (IsAlive == false)
            Cleared?.Invoke();
    }
}