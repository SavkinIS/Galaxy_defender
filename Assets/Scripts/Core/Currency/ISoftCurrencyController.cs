using System;

namespace Core.Currency
{
    public interface ISoftCurrencyController
    {
        int Balance { get; }

        event Action<int> BalanceChanged;

        void Add(int amount);

        /// <summary>
        /// Spends the amount when the balance allows it. Returns false and changes nothing
        /// otherwise.
        /// </summary>
        bool TrySpend(int amount);
    }
}
