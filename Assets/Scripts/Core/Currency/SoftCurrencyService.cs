using System;
using UnityEngine;

namespace Core.Currency
{
    public class SoftCurrencyService : ISoftCurrencyService
    {
        public const string DefaultBalanceKey = "SoftCurrency.Balance";

        private readonly string _balanceKey;

        public SoftCurrencyService()
        {
            _balanceKey = DefaultBalanceKey ;
            Balance = Mathf.Max(0, PlayerPrefs.GetInt(_balanceKey, 99999999));

        }

        public int Balance { get; private set; }

        public event Action<int> BalanceChanged;

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            SetBalance(Balance + amount);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Balance < amount)
                return false;

            SetBalance(Balance - amount);
            return true;
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
