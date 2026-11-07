using System;
using Core.Currency;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class LobbyLifetimeScope : LifetimeScope
{ 
    [SerializeField] private GameStarter  gameStarter;
    [SerializeField] private SoftCurrencyView _softCurrencyView;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(gameStarter);
        builder.RegisterComponent(_softCurrencyView);
    }
}