using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Single owner of <see cref="Time.timeScale"/>. Game speed (time acceleration) only reports
    /// the desired scale here, so a pause can never be overwritten by the acceleration logic.
    /// </summary>
    public sealed class GamePauseService
    {
        private float _desiredTimeScale = 1f;

        public bool IsPaused { get; private set; }

        public event Action<bool> PauseChanged;

        public void ApplyTimeScale(float timeScale)
        {
            _desiredTimeScale = Mathf.Max(0f, timeScale);

            if (IsPaused)
                return;

            Time.timeScale = _desiredTimeScale;
        }

        public void Pause()
        {
            if (IsPaused)
                return;

            IsPaused = true;
            Time.timeScale = 0f;
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            Time.timeScale = _desiredTimeScale;
            PauseChanged?.Invoke(false);
        }

        public void ForceResume()
        {
            if (!IsPaused)
                return;

            Resume();
        }
    }
}
