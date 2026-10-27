using UnityEngine;

namespace TestTask.Features.Clicker.Config
{
    [CreateAssetMenu(menuName = "TestTask/Configs/Clicker", fileName = "ClickerConfig")]
    public sealed class ClickerConfig : ScriptableObject
    {
        [Header("Клик")]
        [SerializeField, Min(1)] private int _currencyPerClick = 1;
        [SerializeField, Min(0)] private int _energyPerClick = 1;

        [Header("Автосбор")]
        [SerializeField, Min(0.1f)] private float _autoCollectInterval = 3f;
        [SerializeField, Min(0)] private int _autoCollectReward = 1;
        [SerializeField, Min(0)] private int _autoCollectEnergyCost = 1;

        [Header("Энергия")]
        [SerializeField, Min(1)] private int _maxEnergy = 1000;
        [SerializeField, Min(0)] private int _startEnergy = 1000;
        [SerializeField, Min(0.1f)] private float _energyRegenInterval = 10f;
        [SerializeField, Min(0)] private int _energyRegenAmount = 10;

        [Header("Эффекты")]
        [SerializeField, Min(0)] private int _clickParticlesCount = 20;
        [SerializeField] private AudioClip _clickSound;
        [SerializeField, Range(0f, 1f)] private float _clickVolume = 0.6f;

        [Header("Тексты")]
        [Tooltip("{0} — количество валюты")]
        [SerializeField] private string _currencyFormat = "Валюта: {0}";
        [Tooltip("{0} — текущая энергия, {1} — максимум")]
        [SerializeField] private string _energyFormat = "Энергия: {0} / {1}";
        [Tooltip("{0} — сколько валюты начислено; текст вылетающей награды")]
        [SerializeField] private string _rewardFormat = "+{0}";
        [Tooltip("Доля энергии, ниже которой шкала подсвечивается как почти пустая")]
        [SerializeField, Range(0f, 1f)] private float _lowEnergyThreshold = 0.1f;

        public int CurrencyPerClick => _currencyPerClick;
        public int EnergyPerClick => _energyPerClick;
        public float AutoCollectInterval => _autoCollectInterval;
        public int AutoCollectReward => _autoCollectReward;
        public int AutoCollectEnergyCost => _autoCollectEnergyCost;
        public int MaxEnergy => _maxEnergy;
        public int StartEnergy => Mathf.Min(_startEnergy, _maxEnergy);
        public float EnergyRegenInterval => _energyRegenInterval;
        public int EnergyRegenAmount => _energyRegenAmount;
        public int ClickParticlesCount => _clickParticlesCount;
        public AudioClip ClickSound => _clickSound;
        public float ClickVolume => _clickVolume;
        public string CurrencyFormat => _currencyFormat;
        public string EnergyFormat => _energyFormat;
        public string RewardFormat => _rewardFormat;
        public float LowEnergyThreshold => _lowEnergyThreshold;
    }
}
