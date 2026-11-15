using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens.Shop
{
    public class SelectedButtons : MonoBehaviour
    {
        [SerializeField] private PanelType _panelType;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _spriteActive;
        [SerializeField] private Sprite _spriteInactive;
        [SerializeField] private Color _colorActive = Color.white;
        [SerializeField] private Color _colorInactive = Color.white;

        private bool _isActive;

        public PanelType PanelType => _panelType;

        public void Initialize(Action<PanelType> onClick)
        {
            if (_button == null)
                return;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(_panelType));
        }

        public void SetActive(bool isActive)
        {
            if (_isActive == isActive)
                return;

            _isActive = isActive;
            Refresh();
        }

        public void Refresh()
        {
            if (_text != null)
                _text.color = _isActive ? _colorActive : _colorInactive;

            if (_image == null)
                return;

            var sprite = _isActive ? _spriteActive : _spriteInactive;
            if (sprite != null)
                _image.sprite = sprite;
        }
    }
}
