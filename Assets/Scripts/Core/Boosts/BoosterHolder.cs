using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoosterHolder : MonoBehaviour
{
    [SerializeField] private List<BoosterView> _boosters;
    private Dictionary<BoosterView.BoosterType, BoosterView> _boostersDic;

    public void Init()
    {
        _boostersDic =  _boosters.ToDictionary(b => b.Type);
    }
}
