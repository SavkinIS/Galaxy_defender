using System;
using Galaxy.Data.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Boosts
{
    public class BoosterView : MonoBehaviour
    {
        [SerializeField] private BoosterType _boosterType;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _countRoot;
        [SerializeField] private TMPro.TextMeshProUGUI _countText;
        //[SerializeField] private TMPro.TextMeshProUGUI _nameText;
        [SerializeField] private GameObject _priceRoot;
        [SerializeField] private TMPro.TextMeshProUGUI _priceText;
        [SerializeField] private GameObject _activeRoot;
        [SerializeField, Min(0.05f)] private float _refreshInterval = 0.25f;

        private BoosterUsageController _controller;
        private float _nextRefreshTime;

        public BoosterType Type => _boosterType;

        private void Awake()
        {
           
        }

        private void Update()
        {
            if (_controller == null)
                return;

            if (Time.unscaledTime < _nextRefreshTime)
                return;

            _nextRefreshTime = Time.unscaledTime + _refreshInterval;
            Refresh();
        }

        public void Init(BoosterUsageController controller)
        {
            if (_controller != null)
                _controller.StateChanged -= OnStateChanged;

            _controller = controller;

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
                _button.onClick.AddListener(OnClick);
            }

            if (_controller != null)
                _controller.StateChanged += OnStateChanged;

            Refresh();
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);

            if (_controller != null)
                _controller.StateChanged -= OnStateChanged;
        }

        private void OnClick()
        {
            _controller?.Click(_boosterType);
        }

        private void OnStateChanged(BoosterType type)
        {
            if (type == _boosterType)
                Refresh();
        }

        private void Refresh()
        {
            if (_controller == null)
                return;

            var count = _controller.GetCount(_boosterType);
            var hasStock = count > 0;

            if (_countRoot != null)
                _countRoot.SetActive(hasStock);

            if (_countText != null)
                _countText.text = count.ToString();

            if (_priceRoot != null)
                _priceRoot.SetActive(!hasStock);

            if (_priceText != null)
                _priceText.text = _controller.GetPrice(_boosterType).ToString();

            if (_activeRoot != null)
                _activeRoot.SetActive(_controller.IsWaitingTarget(_boosterType) && _controller.CanClick(_boosterType));

            if (_button != null)
                _button.interactable = _controller.CanClick(_boosterType);
        }
    }
}
