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

    public bool HasBullets => CurrentBullets > 0;
    public int Size { get;}
    public int MaxBullets { get;}
    public int ColorIndex{ get; }
    public Color Color { get; }
    public int CurrentBullets { get; private set;}
    
    public void DecreaseBullets()
    {
        Spend(1);
    }

    public void Spend(int amount)
    {
        if (amount <= 0)
            return;

        CurrentBullets = Mathf.Max(0, CurrentBullets - amount);

        if (HasBullets == false)
            Cleared?.Invoke();
    }

    public void ResetAmmo()
    {
        CurrentBullets = MaxBullets;
    }
}