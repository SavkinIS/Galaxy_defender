using System.Collections;
using UnityEngine;

namespace Core.Ship.States
{
    /// <summary>
    /// Volley booster state: the ship spends its ammo hitting any block of its color and then
    /// leaves through <see cref="EscapeFromBattle"/>. Runs on unscaled time because the board is
    /// paused while the volley plays.
    /// </summary>
    public class VolleyState : IShipState
    {
        private const float ShotInterval = 0.08f;

        private readonly Ship _ship;

        public VolleyState(Ship ship)
        {
            _ship = ship;
        }

        public void Enter()
        {
            _ship.StartVolleyRoutine(VolleyRoutine());
        }

        private IEnumerator VolleyRoutine()
        {
            var interval = new WaitForSecondsRealtime(ShotInterval);

            while (_ship.IsNoEmpty)
            {
                if (!_ship.TryVolleyShoot())
                    break;

                yield return interval;
            }

            _ship.LeaveAfterVolley();
        }

        public void Exit()
        {
            _ship.StopVolleyRoutine();
        }
    }
}
