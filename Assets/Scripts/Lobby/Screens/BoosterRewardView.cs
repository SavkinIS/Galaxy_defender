using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class BoosterRewardView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _countText;

        public void Bind(Sprite icon, int count)
        {
            if (_icon != null)
                _icon.sprite = icon;

            if (_countText != null)
                _countText.text = count.ToString();
        }
    }
}
