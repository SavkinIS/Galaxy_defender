using System;
using Core.Currency;
using Galaxy.Lobby.Routing;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _changeToGame;
        [SerializeField] private Button _changeToSettings;
        [SerializeField] private Button _changeToShop;
        [SerializeField] private SoftCurrencyView _softCurrencyView;

        public SoftCurrencyView SoftCurrencyView=> _softCurrencyView;
        
        private Action _changeToGameCallback;
        private Action _openShopCallback;
        private Action _openSettingCallback;

        public void OnEnable()
        {
            Subscribe();
        }

        public void OnDisable()
        {
            Unsubscribe();
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

        public void Initialize(Action changeToGameCallback, Action openShopCallback, Action openSettingCallback)
        {
            _changeToGameCallback = changeToGameCallback;
            _openShopCallback = openShopCallback;
            _openSettingCallback = openSettingCallback;
            gameObject.SetActive(false);
        }

        private void Subscribe()
        {
            Unsubscribe();

            _changeToGame.onClick.AddListener(MoveToGame);
            _changeToSettings.onClick.AddListener(MoveToSetting);
            _changeToShop.onClick.AddListener(MoveToShop);
        }

        private void MoveToShop()
        {
            _openShopCallback?.Invoke();
        }

        private void MoveToSetting()
        {
            _openSettingCallback?.Invoke();
        }

        private void MoveToGame()
        {
            _changeToGameCallback?.Invoke();
        }

        private void Unsubscribe()
        {
            _changeToGame.onClick.RemoveAllListeners();
            _changeToSettings.onClick.RemoveAllListeners();
            _changeToShop.onClick.RemoveAllListeners();
        }
    }
}