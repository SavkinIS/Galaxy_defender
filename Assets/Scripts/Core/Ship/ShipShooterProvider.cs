namespace Core.Ship
{
    public class ShipShooterProvider
    {
        private readonly EnemyMap _enemyMap;
        private readonly EnemyCellConnector _enemyCellConnector;

        public ShipShooterProvider(EnemyMap enemyMap, EnemyCellConnector enemyCellConnector)
        {
            _enemyMap = enemyMap;
            _enemyCellConnector = enemyCellConnector;
        }

        public bool CanShoot(PathPoint point, int colorIndex)
        {
            return point != null &&
                   _enemyCellConnector.TryGetLine(point, out var startCellIndex, out var direction) &&
                   _enemyMap.TryGetTargetCellIndex(startCellIndex, direction, colorIndex, out _);
        }

        public bool TryShoot(PathPoint point, int colorIndex)
        {
            if (!_enemyCellConnector.TryGetLine(point, out var startCellIndex, out var direction))
                return false;

            if (!_enemyMap.TryGetTargetCellIndex(startCellIndex, direction, colorIndex, out var targetCellIndex))
                return false;

            _enemyMap.DestroyCell(targetCellIndex);
            return true;
        }

        /// <summary>
        /// Hits any block of the given color. Used by the volley booster, which ignores whether
        /// the block is exposed.
        /// </summary>
        public bool TryVolleyShoot(int colorIndex)
        {
            return _enemyMap.TryDestroyAnyCell(colorIndex);
        }
    }
}