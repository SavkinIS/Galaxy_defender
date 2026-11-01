using UnityEngine;

namespace Core.Boosts
{
    [CreateAssetMenu(menuName = "Boosters/Extra Slot", fileName = "ExtraSlotBooster")]
    public class ExtraSlotBoosterConfigSO : BoosterConfigSO
    {
        [SerializeField, Min(1)] private int _slotsToAdd = 1;

        public override BoosterType Type => BoosterType.ExtraSlot;

        public int SlotsToAdd => Mathf.Max(1, _slotsToAdd);
    }
}
