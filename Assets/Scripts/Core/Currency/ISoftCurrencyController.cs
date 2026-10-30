using System;

namespace Core.Currency
{
    public interface ISoftCurrencyController
    {
        int Balance { get; }

        event Action<int> BalanceChanged;

        void Add(int amount);
    }
}
