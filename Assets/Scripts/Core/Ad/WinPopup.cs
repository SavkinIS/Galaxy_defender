using System;
using Core.Currency;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.Ad
{
    public class WinPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private TMP_Text _rewardText;
        [SerializeField] private string _rewardFormat = "Награда: {0}";
        [SerializeField] private Button _finishButton;
        [SerializeField] private Button _doubleButton;
        [SerializeField] private bool _reloadSceneOnFinish = true;

        private ISoftCurrencyController _currency;
        private int _reward;
        private bool _isDoubled;
        private bool _isShowing;

        public event Action Finished;

        public bool IsShowing => _isShowing;

        public void Init(ISoftCurrencyController currency)
        {
            _currency = currency;
        }

        public void Show(int reward)
        {
            _reward = Mathf.Max(0, reward);
            _isDoubled = false;
            _isShowing = true;

            Grant(_reward);
            RefreshReward();
            RefreshDoubleButton();
            SetVisible(true);
        }

        
        
        private void Awake()
        {
            if (_finishButton != null)
                _finishButton.onClick.AddListener(Finish);

            if (_doubleButton != null)
                _doubleButton.onClick.AddListener(DoubleReward);

            if (_popupRoot != null && _popupRoot != gameObject)
                _popupRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_finishButton != null)
                _finishButton.onClick.RemoveListener(Finish);

            if (_doubleButton != null)
                _doubleButton.onClick.RemoveListener(DoubleReward);
        }

        private void DoubleReward()
        {
            if (!_isShowing || _isDoubled)
                return;

            ADController adController = ADController.Instance;
            if (adController == null)
                return;

            adController.ShowRewardedAd(AdRequestType.DoubleReward, OnDoubleRewardAccepted);
        }

        
        private void OnDoubleRewardAccepted()
        {
            if (!_isShowing || _isDoubled)
                return;

            _isDoubled = true;
            Grant(_reward);
            RefreshReward();
            RefreshDoubleButton();
        }

        private void Finish()
        {
            if (!_isShowing)
                return;

            _isShowing = false;
            SetVisible(false);
            Finished?.Invoke();

            if (_reloadSceneOnFinish)
                RestartScene();
        }

        private static void RestartScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.buildIndex >= 0)
                SceneManager.LoadScene(activeScene.buildIndex);
            else
                SceneManager.LoadScene(activeScene.name);
        }

        private void Grant(int amount)
        {
            _currency?.Add(amount);
        }

        private void RefreshReward()
        {
            if (_rewardText == null)
                return;

            int shownReward = _isDoubled ? _reward * 2 : _reward;
            _rewardText.text = string.Format(_rewardFormat, shownReward);
        }

        private void RefreshDoubleButton()
        {
            if (_doubleButton == null)
                return;

            _doubleButton.gameObject.SetActive(!_isDoubled);
        }

        private void SetVisible(bool isVisible)
        {
            GameObject root = _popupRoot != null ? _popupRoot : gameObject;

            if (!isVisible)
            {
                root.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            root.SetActive(true);
        }
    }
}
