using System;
using TMPro;
using UnityEngine;
using VContainer;

namespace Core.Currency
{
    public class SoftCurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private string _format = "{0}";

        public void OnBalanceChanged(int balance)
        {
            Refresh(balance);
        }

        public void Refresh(int balance)
        {
            if (_balanceText == null)
                return;

            _balanceText.text = string.Format(_format, balance);
        }
    }
}
