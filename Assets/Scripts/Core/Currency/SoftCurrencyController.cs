using System;
using UnityEngine;

namespace Core.Currency
{
    public class SoftCurrencyController : ISoftCurrencyController
    {
        public const string DefaultBalanceKey = "SoftCurrency.Balance";

        private readonly string _balanceKey;

        public SoftCurrencyController(string balanceKey = DefaultBalanceKey)
        {
            _balanceKey = string.IsNullOrEmpty(balanceKey) ? DefaultBalanceKey : balanceKey;
            Balance = Mathf.Max(0, PlayerPrefs.GetInt(_balanceKey, 0));
        }

        public int Balance { get; private set; }

        public event Action<int> BalanceChanged;

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            SetBalance(Balance + amount);
        }

        private void SetBalance(int value)
        {
            int clamped = Mathf.Max(0, value);
            if (clamped == Balance)
                return;

            Balance = clamped;
            PlayerPrefs.SetInt(_balanceKey, Balance);
            PlayerPrefs.Save();
            BalanceChanged?.Invoke(Balance);
        }
    }
}
