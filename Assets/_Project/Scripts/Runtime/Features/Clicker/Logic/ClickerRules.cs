using TestTask.Features.Clicker.Config;
using TestTask.Features.Clicker.Model;

namespace TestTask.Features.Clicker.Logic
{
    /// <summary>
    /// Игровые правила кликера: сколько стоит действие и что за него начисляется.
    /// </summary>
    public sealed class ClickerRules
    {
        private readonly ClickerModel _model;
        private readonly ClickerConfig _config;

        public ClickerRules(ClickerModel model, ClickerConfig config)
        {
            _model = model;
            _config = config;
        }

        /// <returns>false, если не хватило энергии.</returns>
        public bool TryClick()
        {
            if (!_model.TrySpendEnergy(_config.EnergyPerClick))
                return false;

            _model.AddCurrency(_config.CurrencyPerClick);
            return true;
        }

        /// <returns>false, если не хватило энергии и автосбор пропущен.</returns>
        public bool TryAutoCollect()
        {
            if (!_model.TrySpendEnergy(_config.AutoCollectEnergyCost))
                return false;

            _model.AddCurrency(_config.AutoCollectReward);
            return true;
        }

        public void RegenerateEnergy()
        {
            _model.RestoreEnergy(_config.EnergyRegenAmount);
        }
    }
}
