namespace Core.Boosts
{
    /// <summary>
    /// Raises the number of ships that can fight at the same time.
    /// </summary>
    public sealed class ExtraSlotBooster : BoosterBase
    {
        private readonly ExtraSlotBoosterConfigSO _config;

        public ExtraSlotBooster(ExtraSlotBoosterConfigSO config, BoosterContext context)
            : base(config, context)
        {
            _config = config;
        }

        public override bool CanApply()
        {
            return Context.Ships != null;
        }

        public override bool Apply()
        {
            if (Context.Ships == null)
                return false;

            Context.Ships.IncreaseBattleSlots(_config.SlotsToAdd);
            return true;
        }
    }
}
