using NUnit.Framework;
using TestTask.Features.Clicker.Config;
using TestTask.Features.Clicker.Logic;
using TestTask.Features.Clicker.Model;
using TestTask.Features.Clicker.Presenter;
using UnityEngine;

namespace TestTask.Tests
{
    public sealed class ClickerPresenterTests
    {
        private const int MaxEnergy = 10;

        private ClickerConfig _config;
        private ClickerModel _model;
        private FakeClickerView _view;
        private FakeAutoCollectSource _autoCollect;
        private FakeClickerEffects _effects;
        private ClickerPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<ClickerConfig>();
            _view = new FakeClickerView();
            _autoCollect = new FakeAutoCollectSource();
            _effects = new FakeClickerEffects();
        }

        [TearDown]
        public void TearDown()
        {
            _presenter?.Dispose();
            _model?.Dispose();
            Object.DestroyImmediate(_config);
        }

        private void CreatePresenter(int startEnergy = MaxEnergy)
        {
            _model = new ClickerModel(MaxEnergy, startEnergy);
            var rules = new ClickerRules(_model, _config);
            _presenter = new ClickerPresenter(_view, _model, rules, _autoCollect, _effects, _config);
            _presenter.Initialize();
        }

        private string Currency(long value) => string.Format(_config.CurrencyFormat, value);

        private string Energy(int value) => string.Format(_config.EnergyFormat, value, MaxEnergy);

        [Test]
        public void Initialize_HidesViewAndShowsModelState()
        {
            CreatePresenter(startEnergy: 7);

            Assert.IsFalse(_view.IsVisible);
            Assert.AreEqual(Currency(0), _view.CurrencyText);
            Assert.AreEqual(Energy(7), _view.EnergyText);
            Assert.AreEqual(0.7f, _view.EnergyFill, 0.0001f);
            Assert.IsFalse(_view.IsEnergyLow);
        }

        [Test]
        public void Energy_AtOrBelowThreshold_IsMarkedLow()
        {
            CreatePresenter(startEnergy: 1);

            Assert.AreEqual(0.1f, _view.EnergyFill, 0.0001f);
            Assert.IsTrue(_view.IsEnergyLow);
        }

        [Test]
        public void EnterAndExit_ShowAndHideView()
        {
            CreatePresenter();

            _presenter.Enter();
            Assert.IsTrue(_view.IsVisible);

            _presenter.Exit();
            Assert.IsFalse(_view.IsVisible);
        }

        [Test]
        public void Click_WithEnergy_AddsCurrencySpendsEnergyAndPlaysEffects()
        {
            CreatePresenter();
            _presenter.Enter();

            _view.Click();

            Assert.AreEqual(Currency(_config.CurrencyPerClick), _view.CurrencyText);
            Assert.AreEqual(Energy(MaxEnergy - _config.EnergyPerClick), _view.EnergyText);
            Assert.AreEqual(1, _view.FeedbackCount);
            Assert.AreEqual(_config.ClickParticlesCount, _view.LastParticleCount);
            Assert.AreEqual(1, _effects.SoundCount);
            Assert.AreEqual(0, _view.DeniedCount);
        }

        [Test]
        public void Click_WithoutEnergy_PlaysDeniedWithoutReward()
        {
            CreatePresenter(startEnergy: 0);
            _presenter.Enter();

            _view.Click();

            Assert.AreEqual(Currency(0), _view.CurrencyText);
            Assert.AreEqual(1, _view.DeniedCount);
            Assert.AreEqual(0, _view.FeedbackCount);
            Assert.AreEqual(0, _effects.SoundCount);
        }

        [Test]
        public void AutoCollect_WhileTabOpen_PlaysSameEffectsAsClick()
        {
            CreatePresenter();
            _presenter.Enter();

            _autoCollect.Fire();

            Assert.AreEqual(1, _view.FeedbackCount);
            Assert.AreEqual(_config.ClickParticlesCount, _view.LastParticleCount);
            Assert.AreEqual(1, _effects.SoundCount);
        }

        [Test]
        public void AutoCollect_WhileTabHidden_PlaysNoEffects()
        {
            CreatePresenter();

            _autoCollect.Fire();

            Assert.AreEqual(0, _view.FeedbackCount);
            Assert.AreEqual(0, _effects.SoundCount);
        }

        [Test]
        public void Dispose_StopsReactingToClicks()
        {
            CreatePresenter();
            _presenter.Enter();

            _presenter.Dispose();
            _view.Click();

            Assert.AreEqual(0, _effects.SoundCount);
        }
    }
}
