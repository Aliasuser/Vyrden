using UnityEngine;

namespace Vyrden.Progression
{
    /// <summary>
    /// Configuration for how difficulty grows over a run and what it affects. Pure data: the math lives in
    /// <see cref="DifficultyCalculator"/>. A separate asset per difficulty preset (easy / normal / hard)
    /// is the intended way to support selectable difficulty later.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultySettings", menuName = "Vyrden/Progression/Difficulty Settings")]
    public class DifficultySettings : ScriptableObject
    {
        [Header("Growth over time")]
        [Tooltip("Length of one difficulty interval in seconds. Growth is continuous inside an interval, so there are no jumps.")]
        [SerializeField, Min(1f)] private float _secondsPerInterval = 300f;
        [Tooltip("Coefficient added per interval (linear part). The coefficient starts at 1.")]
        [SerializeField, Min(0f)] private float _coefficientPerInterval = 1f;
        [Tooltip("Extra compounding per interval (1 = none, 1.05 = +5% per interval on top of the linear part).")]
        [SerializeField, Min(1f)] private float _compoundPerInterval = 1f;
        [Tooltip("Scales the speed of all growth. For presets: 0.75 easy, 1 normal, 1.5 hard.")]
        [SerializeField, Min(0f)] private float _presetMultiplier = 1f;
        [Tooltip("Highest coefficient allowed. 0 = no cap.")]
        [SerializeField, Min(0f)] private float _maxCoefficient;

        [Header("Enemy stats (per coefficient point above 1)")]
        [Tooltip("0.3 = +30% enemy health per coefficient point.")]
        [SerializeField, Min(0f)] private float _enemyHealthPerCoefficient = 0.3f;
        [Tooltip("0.2 = +20% enemy damage per coefficient point.")]
        [SerializeField, Min(0f)] private float _enemyDamagePerCoefficient = 0.2f;

        [Header("Spawning (per coefficient point above 1)")]
        [Tooltip("0.5 = spawn rate +50% per coefficient point (the interval is divided by 1 + 0.5 per point).")]
        [SerializeField, Min(0f)] private float _spawnRatePerCoefficient = 0.5f;
        [Tooltip("The spawn interval never drops below this fraction of the base interval.")]
        [SerializeField, Range(0.05f, 1f)] private float _minSpawnIntervalMultiplier = 0.3f;
        [Tooltip("Extra max-active enemies per coefficient point (rounded down).")]
        [SerializeField, Min(0f)] private float _maxActivePerCoefficient = 2f;
        [Tooltip("Upper limit for the extra max-active enemies.")]
        [SerializeField, Min(0)] private int _maxActiveBonusCap = 10;

        [Header("Tiers")]
        [Tooltip("Named bands in ascending order of minimum coefficient. For UI and future tier rules.")]
        [SerializeField] private DifficultyTier[] _tiers =
        {
            new DifficultyTier("Easy", 1f),
            new DifficultyTier("Medium", 2f),
            new DifficultyTier("Hard", 3f),
            new DifficultyTier("Very Hard", 4f),
            new DifficultyTier("Extreme", 5f),
            new DifficultyTier("Insane", 6f)
        };

        public float SecondsPerInterval => _secondsPerInterval;
        public float CoefficientPerInterval => _coefficientPerInterval;
        public float CompoundPerInterval => _compoundPerInterval;
        public float PresetMultiplier => _presetMultiplier;
        public float MaxCoefficient => _maxCoefficient;
        public float EnemyHealthPerCoefficient => _enemyHealthPerCoefficient;
        public float EnemyDamagePerCoefficient => _enemyDamagePerCoefficient;
        public float SpawnRatePerCoefficient => _spawnRatePerCoefficient;
        public float MinSpawnIntervalMultiplier => _minSpawnIntervalMultiplier;
        public float MaxActivePerCoefficient => _maxActivePerCoefficient;
        public int MaxActiveBonusCap => _maxActiveBonusCap;
        public DifficultyTier[] Tiers => _tiers;

        private void OnValidate()
        {
            _secondsPerInterval = Mathf.Max(1f, _secondsPerInterval);
            _compoundPerInterval = Mathf.Max(1f, _compoundPerInterval);
        }
    }
}
