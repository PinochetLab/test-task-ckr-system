using System;
using UniRx;

namespace TestTask.Features.Clicker.Model
{
    /// <summary>
    /// Состояние кликера: валюта и энергия. Не зависит от Unity-сцены.
    /// </summary>
    public sealed class ClickerModel : IDisposable
    {
        private readonly ReactiveProperty<long> _currency;
        private readonly ReactiveProperty<int> _energy;

        public ClickerModel(int maxEnergy, int startEnergy)
        {
            if (maxEnergy <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxEnergy));

            MaxEnergy = maxEnergy;
            _currency = new ReactiveProperty<long>(0);
            _energy = new ReactiveProperty<int>(Clamp(startEnergy, 0, maxEnergy));
        }

        public int MaxEnergy { get; }

        public IReadOnlyReactiveProperty<long> Currency => _currency;

        public IReadOnlyReactiveProperty<int> Energy => _energy;

        public bool TrySpendEnergy(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (_energy.Value < amount)
                return false;

            _energy.Value -= amount;
            return true;
        }

        public void AddCurrency(long amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            _currency.Value += amount;
        }

        public void RestoreEnergy(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            _energy.Value = Clamp(_energy.Value + amount, 0, MaxEnergy);
        }

        public void Dispose()
        {
            _currency.Dispose();
            _energy.Dispose();
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
