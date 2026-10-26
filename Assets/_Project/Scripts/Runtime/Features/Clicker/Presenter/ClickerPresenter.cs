using System;
using TestTask.Core.Navigation;
using TestTask.Features.Clicker.Config;
using TestTask.Features.Clicker.Effects;
using TestTask.Features.Clicker.Logic;
using TestTask.Features.Clicker.Model;
using TestTask.Features.Clicker.View;
using UniRx;
using Zenject;

namespace TestTask.Features.Clicker.Presenter
{
    public sealed class ClickerPresenter : ITabPresenter, IInitializable, IDisposable
    {
        private readonly IClickerView _view;
        private readonly ClickerModel _model;
        private readonly ClickerRules _rules;
        private readonly IAutoCollectSource _autoCollect;
        private readonly IClickerEffects _effects;
        private readonly ClickerConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ClickerPresenter(
            IClickerView view,
            ClickerModel model,
            ClickerRules rules,
            IAutoCollectSource autoCollect,
            IClickerEffects effects,
            ClickerConfig config)
        {
            _view = view;
            _model = model;
            _rules = rules;
            _autoCollect = autoCollect;
            _effects = effects;
            _config = config;
        }

        public TabId Id => TabId.Clicker;

        public void Initialize()
        {
            _view.Hide();

            _model.Currency
                .Subscribe(currency => _view.SetCurrency(string.Format(_config.CurrencyFormat, currency)))
                .AddTo(_disposables);

            _model.Energy
                .Subscribe(ShowEnergy)
                .AddTo(_disposables);

            _view.Clicked
                .Subscribe(_ => OnClicked())
                .AddTo(_disposables);

            // Автосбор — это «кнопка нажалась сама»: тот же фидбек, что и при ручном клике.
            // Эффекты показываем только на открытой вкладке, начисление идёт всегда.
            _autoCollect.AutoCollected
                .Where(_ => _view.IsVisible)
                .Subscribe(_ => PlayPressEffects())
                .AddTo(_disposables);
        }

        public void Enter()
        {
            _view.Show();
        }

        public void Exit()
        {
            _view.Hide();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void ShowEnergy(int energy)
        {
            int max = _model.MaxEnergy;
            float fill = max > 0 ? (float)energy / max : 0f;
            string text = string.Format(_config.EnergyFormat, energy, max);

            _view.SetEnergy(text, fill, fill <= _config.LowEnergyThreshold);
        }

        private void OnClicked()
        {
            if (_rules.TryClick())
                PlayPressEffects();
            else
                _view.PlayDenied();
        }

        /// <summary>Полный набор эффектов нажатия: анимация, частицы, звук.</summary>
        private void PlayPressEffects()
        {
            _view.PlayClickFeedback(_config.ClickParticlesCount);
            _effects.PlayClickSound();
        }
    }
}
