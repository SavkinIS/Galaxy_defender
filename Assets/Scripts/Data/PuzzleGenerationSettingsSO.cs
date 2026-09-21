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
    private static readonly SlotRange DefaultSlotRange = new SlotRange(4, 7);

    [SerializeField] private PuzzleDifficulty _difficulty = DefaultDifficulty;
    [SerializeField] private SlotRange _waitingSlots = new SlotRange(4, 7);

    public PuzzleDifficulty Difficulty => NormalizeDifficulty(_difficulty);
    public SlotRange WaitingSlots => _waitingSlots;
    public int ResolvedWaitingSlots => _waitingSlots.GetFor(Difficulty);

    public static PuzzleGenerationSettingsSO CreateRuntimeDefault()
    {
        var settings = CreateInstance<PuzzleGenerationSettingsSO>();
        settings._difficulty = DefaultDifficulty;
        settings._waitingSlots = DefaultSlotRange;
        return settings;
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
