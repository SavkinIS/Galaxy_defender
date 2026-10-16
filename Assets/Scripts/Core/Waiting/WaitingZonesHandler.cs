using System.Collections.Generic;
using UnityEngine;

namespace Core.Waiting
{
    public class WaitingZonesHandler :  MonoBehaviour
    {
        [SerializeField] private List<WaitingZoneView> _zones;
        
        public IReadOnlyList<WaitingZoneView> Zones => _zones;
    }
}