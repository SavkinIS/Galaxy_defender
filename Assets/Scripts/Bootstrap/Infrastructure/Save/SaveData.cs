using System.Collections.Generic;
using Core.Boosts;

public struct SaveData
{
    private int _softValue;
    private int _level;
    private Dictionary<BoosterType, int> Boosters;
    
    public int Level => _level;
    public int SoftValue => _softValue;
}