using UnityEngine;

[CreateAssetMenu(fileName = "GameSessionSettings", menuName = "Game/Session Settings")]
public sealed class GameSessionSettingsSO : ScriptableObject
{
    public const float DefaultAcceleratedTimeScale = 2f;
    public const float DefaultFullBattleTimeScale = 1.2f;

    [SerializeField, Min(1f)] private float _acceleratedTimeScale = DefaultAcceleratedTimeScale;
    [SerializeField, Min(1f)] private float _fullBattleTimeScale = DefaultFullBattleTimeScale;

    public float AcceleratedTimeScale => Mathf.Max(1f, _acceleratedTimeScale);
    public float FullBattleTimeScale => Mathf.Max(1f, _fullBattleTimeScale);

    private void OnValidate()
    {
        _acceleratedTimeScale = Mathf.Max(1f, _acceleratedTimeScale);
        _fullBattleTimeScale = Mathf.Max(1f, _fullBattleTimeScale);
    }
}
