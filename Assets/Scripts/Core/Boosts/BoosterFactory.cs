namespace Core.Boosts
{
    public static class BoosterFactory
    {
        public static IBooster Create(BoosterConfigSO config, BoosterContext context)
        {
            switch (config)
            {
                case EscapeSelectedBoosterConfigSO escapeSelected:
                    return new EscapeSelectedBooster(escapeSelected, context);
                case DeleteColorBoosterConfigSO deleteColor:
                    return new DeleteColorBooster(deleteColor, context);
                case ExtraSlotBoosterConfigSO extraSlot:
                    return new ExtraSlotBooster(extraSlot, context);
                case VolleyBoosterConfigSO volley:
                    return new VolleyBooster(volley, context);
                default:
                    return null;
            }
        }
    }
}
