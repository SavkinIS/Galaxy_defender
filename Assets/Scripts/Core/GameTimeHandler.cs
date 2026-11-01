using UnityEngine;

namespace Core
{
    public sealed class GameTimeHandler
    {
        private readonly float _initialTimeScale;
        private readonly float _parkingEmptyTimeScale;
        private readonly float _fullBattleTimeScale;
        private readonly GamePauseService _pauseService;
        private float _currentMultiplier = 1f;

        public GameTimeHandler(float parkingEmptyTimeScale, float fullBattleTimeScale,
            GamePauseService pauseService = null)
        {
            // A leftover zero scale (an interrupted session) must not freeze the next one.
            _initialTimeScale = Time.timeScale > 0.01f ? Time.timeScale : 1f;
            _parkingEmptyTimeScale = Mathf.Max(1f, parkingEmptyTimeScale);
            _fullBattleTimeScale = Mathf.Max(1f, fullBattleTimeScale);
            _pauseService = pauseService;
        }

        public void UpdateAcceleration(bool allBattleSlotsOccupied, bool shipsRemainOnParking)
        {
            _currentMultiplier = !shipsRemainOnParking
                ? _parkingEmptyTimeScale
                : allBattleSlotsOccupied ? _fullBattleTimeScale : 1f;

            Apply();
        }

        public void Reset()
        {
            _currentMultiplier = 1f;
            Apply();
        }

        private void Apply()
        {
            var targetTimeScale = _initialTimeScale * _currentMultiplier;

            if (_pauseService != null)
            {
                _pauseService.ApplyTimeScale(targetTimeScale);
                return;
            }

            Time.timeScale = targetTimeScale;
        }
    }
}
