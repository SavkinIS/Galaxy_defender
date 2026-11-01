using UnityEngine;

namespace Core.Boosts
{
    [CreateAssetMenu(menuName = "Boosters/Escape Selected", fileName = "EscapeSelectedBooster")]
    public class EscapeSelectedBoosterConfigSO : BoosterConfigSO
    {
        public override BoosterType Type => BoosterType.EscapeSelected;
    }
}
