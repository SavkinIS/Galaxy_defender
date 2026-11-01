using System.Collections;
using System.Collections.Generic;
using Core.Ship;

namespace Core.Boosts
{
    /// <summary>
    /// Pauses the board and lets every ship from the waiting zones shoot its ammo at the blocks of
    /// its own color, one ship after another, ignoring whether a block is exposed. When the volley
    /// is over the ships leave and the battle end is checked once.
    /// </summary>
    public sealed class VolleyBooster : BoosterBase
    {
        private const int MaxFramesPerShip = 60 * 10;

        public VolleyBooster(VolleyBoosterConfigSO config, BoosterContext context)
            : base(config, context)
        {
        }

        public override bool CanApply()
        {
            return Context.WaitingZones != null && Context.WaitingZones.AreAllStarterZonesOccupied;
        }

        public override bool Apply()
        {
            if (!CanApply() || Context.CoroutineRunner == null)
                return false;

            Context.CoroutineRunner.StartCoroutine(RunVolley());
            return true;
        }

        private IEnumerator RunVolley()
        {
            var ships = Context.Ships;

            if (ships == null)
                yield break;

            Context.PauseService?.Pause();
            ships.SuspendBattleCompletion(true);

            var targets = CollectTargets(ships);

            for (var index = 0; index < targets.Count; index++)
            {
                var ship = targets[index];
                ship.StartVolley();

                var frames = 0;
                while (!ship.IsVolleyFinished)
                {
                    // Safety net: a stuck volley must not leave the board paused forever.
                    if (frames++ > MaxFramesPerShip)
                        break;

                    yield return null;
                }
            }

            Context.PauseService?.Resume();
            ships.SuspendBattleCompletion(false);
            ships.CheckBattleCompletion();
        }

        private List<Core.Ship.Ship> CollectTargets(IShipsController ships)
        {
            var targets = new List<Core.Ship.Ship>();

            if (Context.WaitingZones == null)
                return targets;

            var occupied = Context.WaitingZones.OccupiedShips;

            for (var index = 0; index < occupied.Count; index++)
            {
                if (!ships.TryGetShip(occupied[index], out var ship))
                    continue;

                if (ship == null || !ship.IsNoEmpty)
                    continue;

                targets.Add(ship);
            }

            return targets;
        }
    }
}
