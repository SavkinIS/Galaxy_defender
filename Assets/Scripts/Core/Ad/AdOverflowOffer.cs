using System;

namespace Core.Ad
{
    /// <summary>
    /// Предложение посмотреть рекламу, когда зоны ожидания переполнены.
    /// </summary>
    public class AdOverflowOffer : AdOfferPopup
    {
        public const string Message = "Зоны ожидания переполнены. Посмотреть рекламу, чтобы открыть дополнительное место?";

        protected override string Prompt => Message;
    }
}
