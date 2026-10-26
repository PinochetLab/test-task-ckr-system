using System;
using TestTask.Features.Clicker.Config;
using UniRx;
using Zenject;

namespace TestTask.Features.Clicker.Logic
{
    /// <summary>
    /// Таймеры кликера: автосбор и восстановление энергии.
    /// Работают постоянно, независимо от открытой вкладки.
    /// </summary>
    public sealed class ClickerTicker : IAutoCollectSource, IInitializable, IDisposable
    {
        private readonly ClickerRules _rules;
        private readonly ClickerConfig _config;
        private readonly Subject<Unit> _autoCollected = new Subject<Unit>();

        private CompositeDisposable _timers;

        public ClickerTicker(ClickerRules rules, ClickerConfig config)
        {
            _rules = rules;
            _config = config;
        }

        /// <summary>Срабатывает при успешном автосборе.</summary>
        public IObservable<Unit> AutoCollected => _autoCollected;

        public bool IsRunning => _timers != null;

        public void Initialize()
        {
            Start();
        }

        public void Start()
        {
            if (IsRunning)
                return;

            _timers = new CompositeDisposable();

            Observable.Interval(TimeSpan.FromSeconds(_config.AutoCollectInterval))
                .Subscribe(_ => OnAutoCollectTick())
                .AddTo(_timers);

            Observable.Interval(TimeSpan.FromSeconds(_config.EnergyRegenInterval))
                .Subscribe(_ => _rules.RegenerateEnergy())
                .AddTo(_timers);
        }

        public void Stop()
        {
            _timers?.Dispose();
            _timers = null;
        }

        public void Dispose()
        {
            Stop();
            _autoCollected.OnCompleted();
            _autoCollected.Dispose();
        }

        private void OnAutoCollectTick()
        {
            if (_rules.TryAutoCollect())
                _autoCollected.OnNext(Unit.Default);
        }
    }
}
