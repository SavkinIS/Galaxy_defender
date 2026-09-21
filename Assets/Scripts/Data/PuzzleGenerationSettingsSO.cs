using System;
using UnityEngine;

public enum PuzzleDifficulty
{
    Training = 1,
    Easy = 2,
    Medium = 3,
    Hard = 4,
    Hardcore = 5
}

[Serializable]
public struct SlotRange
{
    public const int MinimumAllowedSlots = 1;
    public const int MaximumAllowedSlots = 7;

    [SerializeField, Min(MinimumAllowedSlots)] private int _minimum;
    [SerializeField, Min(MinimumAllowedSlots)] private int _maximum;

    public int Minimum => Mathf.Clamp(_minimum, MinimumAllowedSlots, MaximumAllowedSlots);
    public int Maximum => Mathf.Clamp(Mathf.Max(_maximum, _minimum), MinimumAllowedSlots, MaximumAllowedSlots);

    public int GetFor(PuzzleDifficulty difficulty)
    {
        int difficultyIndex = (int)difficulty - (int)PuzzleDifficulty.Training;
        int difficultySteps = (int)PuzzleDifficulty.Hardcore - (int)PuzzleDifficulty.Training;
        float progress = difficultySteps <= 0 ? 0f : (float)difficultyIndex / difficultySteps;
        int slots = Mathf.RoundToInt(Mathf.Lerp(Maximum, Minimum, progress));
        return Mathf.Clamp(slots, Minimum, Maximum);
    }

    public SlotRange(int minimum, int maximum)
    {
        _minimum = Mathf.Clamp(minimum, MinimumAllowedSlots, MaximumAllowedSlots);
        _maximum = Mathf.Clamp(Mathf.Max(maximum, _minimum), MinimumAllowedSlots, MaximumAllowedSlots);
    }
}

[CreateAssetMenu(fileName = "PuzzleGenerationSettings", menuName = "Level Generator/Puzzle Generation Settings")]
public sealed class PuzzleGenerationSettingsSO : ScriptableObject
{
    private const PuzzleDifficulty DefaultDifficulty = PuzzleDifficulty.Medium;
    private const int DefaultSmallShipSize = 1;
    private const int DefaultMediumShipSize = 2;
    private const int DefaultLargeShipSize = 3;
    private const int DefaultSmallShipAmmo = 10;
    private const int DefaultMediumShipAmmo = 20;
    private const int DefaultLargeShipAmmo = 30;
    private static readonly SlotRange DefaultSlotRange = new SlotRange(4, 7);

    [Header("Level balance")]
    [SerializeField] private PuzzleDifficulty _difficulty = DefaultDifficulty;
    [SerializeField] private SlotRange _waitingSlots = new SlotRange(4, 7);

    [Header("Ship balance")]
    [SerializeField, Min(1)] private int _smallShipSize = DefaultSmallShipSize;
    [SerializeField, Min(1)] private int _mediumShipSize = DefaultMediumShipSize;
    [SerializeField, Min(1)] private int _largeShipSize = DefaultLargeShipSize;
    [SerializeField, Min(1)] private int _smallShipAmmo = DefaultSmallShipAmmo;
    [SerializeField, Min(1)] private int _mediumShipAmmo = DefaultMediumShipAmmo;
    [SerializeField, Min(1)] private int _largeShipAmmo = DefaultLargeShipAmmo;

    public PuzzleDifficulty Difficulty => NormalizeDifficulty(_difficulty);
    public SlotRange WaitingSlots => _waitingSlots;
    public int ResolvedWaitingSlots => _waitingSlots.GetFor(Difficulty);
    public int SmallShipSize => Mathf.Max(1, _smallShipSize);
    public int MediumShipSize => Mathf.Max(1, _mediumShipSize);
    public int LargeShipSize => Mathf.Max(1, _largeShipSize);
    public int SmallShipAmmo => Mathf.Max(1, _smallShipAmmo);
    public int MediumShipAmmo => Mathf.Max(1, _mediumShipAmmo);
    public int LargeShipAmmo => Mathf.Max(1, _largeShipAmmo);

    public static PuzzleGenerationSettingsSO CreateRuntimeDefault()
    {
        var settings = CreateInstance<PuzzleGenerationSettingsSO>();
        settings._difficulty = DefaultDifficulty;
        settings._waitingSlots = DefaultSlotRange;
        settings._smallShipSize = DefaultSmallShipSize;
        settings._mediumShipSize = DefaultMediumShipSize;
        settings._largeShipSize = DefaultLargeShipSize;
        settings._smallShipAmmo = DefaultSmallShipAmmo;
        settings._mediumShipAmmo = DefaultMediumShipAmmo;
        settings._largeShipAmmo = DefaultLargeShipAmmo;
        return settings;
    }

    private void OnValidate()
    {
        _waitingSlots = new SlotRange(_waitingSlots.Minimum, _waitingSlots.Maximum);
        _smallShipSize = Mathf.Max(1, _smallShipSize);
        _mediumShipSize = Mathf.Max(_smallShipSize + 1, _mediumShipSize);
        _largeShipSize = Mathf.Max(_mediumShipSize + 1, _largeShipSize);
        _smallShipAmmo = Mathf.Max(1, _smallShipAmmo);
        _mediumShipAmmo = Mathf.Max(1, _mediumShipAmmo);
        _largeShipAmmo = Mathf.Max(1, _largeShipAmmo);
    }

    private static PuzzleDifficulty NormalizeDifficulty(PuzzleDifficulty difficulty)
    {
        int value = Mathf.Clamp(
            (int)difficulty,
            (int)PuzzleDifficulty.Training,
            (int)PuzzleDifficulty.Hardcore);
        return (PuzzleDifficulty)value;
    }
}
