using UnityEngine;

namespace Core.Boosts
{
    [CreateAssetMenu(menuName = "Boosters/Volley", fileName = "VolleyBooster")]
    public class VolleyBoosterConfigSO : BoosterConfigSO
    {
        public override BoosterType Type => BoosterType.Volley;
    }
}
