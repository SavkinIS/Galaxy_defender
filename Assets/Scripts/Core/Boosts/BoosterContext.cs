using Core.Ship;
using Core.Waiting;
using UnityEngine;

namespace Core.Boosts
{
    public sealed class BoosterContext
    {
        public BoosterContext(IShipsController ships, IWaitingZonesController waitingZones,
            EnemyMap enemyMap, GamePauseService pauseService, MonoBehaviour coroutineRunner)
        {
            Ships = ships;
            WaitingZones = waitingZones;
            EnemyMap = enemyMap;
            PauseService = pauseService;
            CoroutineRunner = coroutineRunner;
        }

        public IShipsController Ships { get; }
        public IWaitingZonesController WaitingZones { get; }
        public EnemyMap EnemyMap { get; }
        public GamePauseService PauseService { get; }
        public MonoBehaviour CoroutineRunner { get; }
    }
}
