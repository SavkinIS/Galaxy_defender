using TMPro;
using UnityEngine;

namespace Core.Currency
{
    public class SoftCurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private string _format = "{0}";

        private ISoftCurrencyController _currency;

        public void Init(ISoftCurrencyController currency)
        {
            if (_currency != null)
                _currency.BalanceChanged -= OnBalanceChanged;

            _currency = currency;

            if (_currency != null)
                _currency.BalanceChanged += OnBalanceChanged;

            Refresh();
        }

        private void OnDestroy()
        {
            if (_currency != null)
                _currency.BalanceChanged -= OnBalanceChanged;
        }

        private void OnBalanceChanged(int balance)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_balanceText == null)
                return;

            int balance = _currency != null ? _currency.Balance : 0;
            _balanceText.text = string.Format(_format, balance);
        }
    }
}
