using System;
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
        public long Currency { get; private set; } = -1;
        public int Energy { get; private set; } = -1;
        public int MaxEnergy { get; private set; } = -1;
        public int FeedbackCount { get; private set; }
        public int LastParticleCount { get; private set; }
        public int DeniedCount { get; private set; }

        public void Click() => _clicked.OnNext(Unit.Default);

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
        public void SetCurrency(long value) => Currency = value;

        public void SetEnergy(int current, int max)
        {
            Energy = current;
            MaxEnergy = max;
        }

        public void PlayClickFeedback(int particleCount)
        {
            FeedbackCount++;
            LastParticleCount = particleCount;
        }

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
