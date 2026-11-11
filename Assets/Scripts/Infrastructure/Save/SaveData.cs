using System;
using System.Collections.Generic;
using Galaxy.Data.Infrastructure;

[Serializable]
public class SaveData
{
    public const int CurrentVersion = 2;
    public int Version = CurrentVersion;
    public int Level = 1;
    public int SoftValue;
    public Dictionary<BoosterType, int> Boosters = new();
    public long FirstLaunchUtc;
    public long LastDailyRewardUtc;
    public int LastEnemyTypeIndex = -1; 
    public bool TutorialCompleted;
    public float MusicVolume = 1f;
    public float SfxVolume = 1f;
    public string Language = "";

    // Paid content
    public bool NoAds;
    public List<string> PurchasedProducts = new();
}
