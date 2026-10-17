using UnityEngine;

namespace Core
{
    public sealed class GameTimeHandler
    {
        private readonly float _initialTimeScale;
        private readonly float _parkingEmptyTimeScale;
        private readonly float _fullBattleTimeScale;
        private float _currentMultiplier = 1f;

        public GameTimeHandler(float parkingEmptyTimeScale, float fullBattleTimeScale)
        {
            _initialTimeScale = Time.timeScale;
            _parkingEmptyTimeScale = Mathf.Max(1f, parkingEmptyTimeScale);
            _fullBattleTimeScale = Mathf.Max(1f, fullBattleTimeScale);
        }

        public void UpdateAcceleration(bool allBattleSlotsOccupied, bool shipsRemainOnParking)
        {
            float multiplier = !shipsRemainOnParking
                ? _parkingEmptyTimeScale
                : allBattleSlotsOccupied ? _fullBattleTimeScale : 1f;

            if (Mathf.Approximately(_currentMultiplier, multiplier))
                return;

            Time.timeScale = _initialTimeScale * multiplier;
            _currentMultiplier = multiplier;
        }

        public void Reset()
        {
            UpdateAcceleration(false, true);
        }
    }
}
