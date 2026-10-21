using System;

namespace Core.Ad
{
    /// <summary>
    /// Предложение посмотреть рекламу, чтобы разблокировать зону, по которой кликнул игрок.
    /// </summary>
    public class AdUnlockZoneOffer : AdOfferPopup
    {
        public const string Message = "Посмотреть рекламу, чтобы разблокировать зону?";

        protected override string Prompt => Message;
    }
}
