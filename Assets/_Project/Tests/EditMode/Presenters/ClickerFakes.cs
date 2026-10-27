using System;
using System.Collections.Generic;
using TestTask.Features.Clicker.Effects;
using TestTask.Features.Clicker.Logic;
using TestTask.Features.Clicker.View;
using UniRx;

namespace TestTask.Tests
{
    internal sealed class FakeClickerView : IClickerView
    {
        private readonly Subject<Unit> _clicked = new Subject<Unit>();

        public IObservable<Unit> Clicked => _clicked;
        public bool IsVisible { get; private set; } = true;
        public string CurrencyText { get; private set; }
        public string EnergyText { get; private set; }
        public float EnergyFill { get; private set; } = -1f;
        public bool IsEnergyLow { get; private set; }
        public int FeedbackCount { get; private set; }
        public int LastParticleCount { get; private set; }
        public int DeniedCount { get; private set; }
        public List<string> PointerRewards { get; } = new List<string>();

        public void Click() => _clicked.OnNext(Unit.Default);

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
        public void SetCurrency(string text) => CurrencyText = text;

        public void SetEnergy(string text, float fill, bool isLow)
        {
            EnergyText = text;
            EnergyFill = fill;
            IsEnergyLow = isLow;
        }

        public void PlayClickFeedback(int particleCount)
        {
            FeedbackCount++;
            LastParticleCount = particleCount;
        }

        public List<string> CenterRewards { get; } = new List<string>();

        public void ShowRewardAtPointer(string text) => PointerRewards.Add(text);
        public void ShowRewardAtCenter(string text) => CenterRewards.Add(text);

        public void PlayDenied() => DeniedCount++;
    }

    internal sealed class FakeAutoCollectSource : IAutoCollectSource
    {
        private readonly Subject<Unit> _autoCollected = new Subject<Unit>();

        public IObservable<Unit> AutoCollected => _autoCollected;

        public void Fire() => _autoCollected.OnNext(Unit.Default);
    }

    internal sealed class FakeClickerEffects : IClickerEffects
    {
        public int SoundCount { get; private set; }

        public void PlayClickSound() => SoundCount++;
    }
}
