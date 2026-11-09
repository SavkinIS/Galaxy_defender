using System;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _toMainMenuBtn;
        private Action _toMainMenu;

        public void OnEnable()
        {
            _toMainMenuBtn.onClick.AddListener(ToMainMenu);
        }
        
        public void OnDisable()
        {
            _toMainMenuBtn.onClick.RemoveAllListeners();
        }

        public void Initialize(Action toMainMenu)
        {
            _toMainMenu = toMainMenu;
            gameObject.SetActive(false);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
        }
        
        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        
        private void ToMainMenu()
        {
            _toMainMenu?.Invoke();
        }
    }
}