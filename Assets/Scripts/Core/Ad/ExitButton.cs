using System;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    private Action _onClickCallback;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void SetToLobbyCallback(Action callback)
    {
        _onClickCallback = callback;
    }      
    
    private void OnClick()
    {
        _onClickCallback?.Invoke();
    }
}
