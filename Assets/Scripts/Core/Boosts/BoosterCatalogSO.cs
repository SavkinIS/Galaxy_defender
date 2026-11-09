using System.Collections.Generic;
using Galaxy.Data.Infrastructure;
using UnityEngine;

namespace Core.Boosts
{
    /// <summary>
    /// Single source of booster configs that lives in the asset database, not in a scene.
    /// Registered in the root scope so both the lobby shop and the game read the same data.
    /// </summary>
    [CreateAssetMenu(menuName = "Galaxy/Booster Catalog", fileName = "BoosterCatalog")]
    public sealed class BoosterCatalogSO : ScriptableObject
    {
        [SerializeField] private List<BoosterConfigSO> _boosters = new List<BoosterConfigSO>();

        public IReadOnlyList<BoosterConfigSO> Boosters => _boosters;

        public bool TryGet(BoosterType type, out BoosterConfigSO config)
        {
            for (var index = 0; index < _boosters.Count; index++)
            {
                var candidate = _boosters[index];
                if (candidate != null && candidate.Type == type)
                {
                    config = candidate;
                    return true;
                }
            }

            config = null;
            return false;
        }
    }
}
