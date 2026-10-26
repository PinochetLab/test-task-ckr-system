using System;
using TestTask.Core.Navigation;
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
        private readonly ClickerView _view;
        private readonly ClickerModel _model;
        private readonly ClickerRules _rules;
        private readonly ClickerTicker _ticker;
        private readonly ClickerEffectsPlayer _effects;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ClickerPresenter(
            ClickerView view,
            ClickerModel model,
            ClickerRules rules,
            ClickerTicker ticker,
            ClickerEffectsPlayer effects)
        {
            _view = view;
            _model = model;
            _rules = rules;
            _ticker = ticker;
            _effects = effects;
        }

        public TabId Id => TabId.Clicker;

        public void Initialize()
        {
            _view.Hide();

            _model.Currency
                .Subscribe(_view.SetCurrency)
                .AddTo(_disposables);

            _model.Energy
                .Subscribe(energy => _view.SetEnergy(energy, _model.MaxEnergy))
                .AddTo(_disposables);

            _view.Clicked
                .Subscribe(_ => OnClicked())
                .AddTo(_disposables);

            // Автосбор — это «кнопка нажалась сама»: тот же фидбек, что и при ручном клике.
            // Эффекты показываем только на открытой вкладке, начисление идёт всегда.
            _ticker.AutoCollected
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

        private void OnClicked()
        {
            if (_rules.TryClick())
                PlayPressEffects();
            else
                _view.PlayDenied();
        }

        /// <summary>Полный набор эффектов нажатия: анимация кнопки, звук, частицы, летящая валюта.</summary>
        private void PlayPressEffects()
        {
            _view.PlayClickFeedback();
            _effects.PlayClick(_view.ClickParticles);
        }
    }
}
