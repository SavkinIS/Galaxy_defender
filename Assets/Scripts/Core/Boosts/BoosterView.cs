using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterView : MonoBehaviour
{
    public enum BoosterType
    {
        EscapeSelected, //выпускает любой корабль
        Blend, //перемешать
        DeleteColor // Удалить блоки определенного цвета
    }
    
    [SerializeField] private  BoosterType _boosterType;
    [SerializeField] private Button _button;
    
    public BoosterType Type => _boosterType;
}
