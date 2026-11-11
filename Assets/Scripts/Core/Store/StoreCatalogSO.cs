using System.Collections.Generic;
using UnityEngine;

namespace Core.Store
{
    [CreateAssetMenu(menuName = "Galaxy/Store Catalog", fileName = "StoreCatalog")]
    public sealed class StoreCatalogSO : ScriptableObject
    {
        [SerializeField] private List<StoreOfferSO> _offers = new List<StoreOfferSO>();

        public IReadOnlyList<StoreOfferSO> Offers => _offers;

        public bool TryGetByProductId(string productId, out StoreOfferSO offer)
        {
            for (var index = 0; index < _offers.Count; index++)
            {
                var candidate = _offers[index];
                if (candidate != null && candidate.ProductId == productId)
                {
                    offer = candidate;
                    return true;
                }
            }

            offer = null;
            return false;
        }
    }
}
