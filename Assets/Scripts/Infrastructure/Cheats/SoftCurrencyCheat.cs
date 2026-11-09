#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Core.Currency;
using UnityEngine;
using VContainer;

/// <summary>
/// Разработческий чит: по клавише добавляет софт-валюту через рабочий сервис,
/// поэтому баланс обновляется в UI сразу и уходит в сохранение штатным путём.
/// В релизный билд не попадает.
/// </summary>
public sealed class SoftCurrencyCheat : MonoBehaviour
{
    [SerializeField] private KeyCode _addKey = KeyCode.F1;
    [SerializeField] private int _addAmount = 1000;

    private ISoftCurrencyService _currency;

    [Inject]
    public void Construct(ISoftCurrencyService currency)
    {
        _currency = currency;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_addKey) == false)
            return;

        _currency.Add(_addAmount);
        Log.Info($"[Cheat] +{_addAmount} soft, balance = {_currency.Balance}");
    }
}
#endif
