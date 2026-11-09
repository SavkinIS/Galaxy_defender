using System;
using System.Collections.Generic;
using Core.Currency;
using VContainer.Unity;

namespace Galaxy.Infrastructure.UI.Presenters
{
    public class SoftCurrencyPresenter : IStartable, IDisposable
    {
        private readonly IReadOnlyList<SoftCurrencyView> _views;
        private readonly ISoftCurrencyService _softCurrencyService;

        public SoftCurrencyPresenter(IReadOnlyList<SoftCurrencyView> views, ISoftCurrencyService softCurrencyService)
        {
            _views = views;
            _softCurrencyService = softCurrencyService;
        }

        public void Start()
        {
            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_softCurrencyService == null)
                return;
            
            foreach (var view in _views)
                _softCurrencyService.BalanceChanged -= view.OnBalanceChanged;
        }
        
        private void Subscribe()
        {
            if (_softCurrencyService == null)
                return;
            
            foreach (var view in _views)
            {
                _softCurrencyService.BalanceChanged += view.OnBalanceChanged;
                view.Refresh(_softCurrencyService.Balance);
            }
            
            
        }
    }
}