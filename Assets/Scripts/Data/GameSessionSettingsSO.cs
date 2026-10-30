using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionSettings", menuName = "Game/Session Settings")]
public sealed class GameSessionSettingsSO : ScriptableObject
{
    public const float DefaultAcceleratedTimeScale = 2f;
    public const float DefaultFullBattleTimeScale = 1.2f;
    public const int DefaultInitialLockedZonesCount = 2;
    public const int DefaultWinReward = 1000;

    [SerializeField, Min(1f)] private float _acceleratedTimeScale = DefaultAcceleratedTimeScale;
    [SerializeField, Min(1f)] private float _fullBattleTimeScale = DefaultFullBattleTimeScale;
    [SerializeField, Min(0)] private int _initialLockedZonesCount = DefaultInitialLockedZonesCount;
    [SerializeField, Min(0)] private int _winReward = DefaultWinReward;

    public float AcceleratedTimeScale => Mathf.Max(1f, _acceleratedTimeScale);
    public float FullBattleTimeScale => Mathf.Max(1f, _fullBattleTimeScale);
    public int InitialLockedZonesCount => _initialLockedZonesCount;
    public int WinReward => _winReward > 0 ? _winReward : DefaultWinReward;

    private void OnValidate()
    {
        _acceleratedTimeScale = Mathf.Max(1f, _acceleratedTimeScale);
        _fullBattleTimeScale = Mathf.Max(1f, _fullBattleTimeScale);
        _initialLockedZonesCount = Mathf.Max(0, _initialLockedZonesCount);
        _winReward = Mathf.Max(0, _winReward);
    }
}
