using System;

namespace Core.Ad
{
    public static class AdRequests
    {
        private const string DoubleRewardMessage = "Посмотреть рекламу, чтобы удвоить награду?";
        private const string UnlockZoneMessage = "Посмотреть рекламу, чтобы разблокировать зону?";
        private const string OverflowMessage = "Зоны ожидания переполнены. Посмотреть рекламу, чтобы открыть дополнительное место?";
        private const string DefaultMessage = "Посмотреть рекламу?";

        public static AdRequest Create(AdRequestType type, Action onGranted, Action onDeclined = null)
        {
            return new AdRequest(type, GetMessage(type), onGranted, onDeclined, null, RequiresPopup(type));
        }

        public static string GetMessage(AdRequestType type)
        {
            switch (type)
            {
                case AdRequestType.DoubleReward:
                    return DoubleRewardMessage;
                case AdRequestType.UnlockZone:
                    return UnlockZoneMessage;
                case AdRequestType.Overflow:
                    return OverflowMessage;
                default:
                    return DefaultMessage;
            }
        }
        
        private static bool RequiresPopup(AdRequestType type)
        {
            return type != AdRequestType.DoubleReward;
        }
    }
}
