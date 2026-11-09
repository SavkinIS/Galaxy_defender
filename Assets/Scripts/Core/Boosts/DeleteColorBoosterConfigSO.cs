using Galaxy.Data.Infrastructure;
using UnityEngine;

namespace Core.Boosts
{
    [CreateAssetMenu(menuName = "Boosters/Delete Color", fileName = "DeleteColorBooster")]
    public class DeleteColorBoosterConfigSO : BoosterConfigSO
    {
        public override BoosterType Type => BoosterType.DeleteColor;
    }
}
