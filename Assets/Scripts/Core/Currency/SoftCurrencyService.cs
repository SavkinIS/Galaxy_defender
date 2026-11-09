using System;
using UnityEngine;

namespace Core.Currency
{
    public class SoftCurrencyService : ISoftCurrencyService
    {
        private readonly ISaveService _saveService;

        public SoftCurrencyService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public int Balance => _saveService.GetSaveData().SoftValue;

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

            _saveService.GetSaveData().SoftValue = clamped;
            _saveService.Save();
            
            BalanceChanged?.Invoke(Balance);
        }
    }
}
