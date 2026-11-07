using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Ad
{
    public class GameOverPopup : MonoBehaviour
    {
        [SerializeField] private Button _endBtn;
        private Action _onClickCallBack;

        private void Start()
        {
            _endBtn.onClick.RemoveAllListeners();
            _endBtn.onClick.AddListener(OnClick);
        }


        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void SetToLobbyCallback(Action changeScene)
        {
            _onClickCallBack = changeScene;
        }
        
        
        private void OnClick()
        {
            _onClickCallBack?.Invoke();
        }
    }
}
