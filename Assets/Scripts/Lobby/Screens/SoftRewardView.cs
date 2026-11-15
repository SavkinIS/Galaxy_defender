using TMPro;
using UnityEngine;

namespace Galaxy.Lobby.Screens
{
    public class SoftRewardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countText;

        public void Bind(int count)
        {
            if (_countText != null)
                _countText.text = count.ToString();

            gameObject.SetActive(count > 0);
        }
    }
}