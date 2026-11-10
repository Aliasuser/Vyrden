using System;
using UnityEngine;

namespace Vyrden.Progression
{
    /// <summary>
    /// A character's experience and level. <see cref="AddExperience"/> is the single entry point for
    /// gaining experience (host-only in multiplayer later). Stat growth and UI listen to the events.
    /// </summary>
    public class CharacterExperience : MonoBehaviour
    {
        [SerializeField] private ExperienceCurveDefinition _curve;
        [SerializeField, Min(1)] private int _startingLevel = 1;

        private int _level;
        private float _experienceIntoLevel;
        private float _totalExperience;
        private bool _initialized;

        /// <summary>Raised after experience is added. Args: amount gained, source (can be null).</summary>
        public event Action<float, GameObject> ExperienceGained;

        /// <summary>Raised whenever progress changes. Args: experience into the level, experience the level needs.</summary>
        public event Action<float, float> ExperienceChanged;

        /// <summary>Raised once per gain that raises the level, even when it covers several levels.</summary>
        public event Action<LevelUpInfo> LeveledUp;

        /// <summary>Raised whenever the level changes for any reason (level-up or reset). Args: previous, current.</summary>
        public event Action<int, int> LevelChanged;

        public int Level
        {
            get
            {
                EnsureInitialized();
                return _level;
            }
        }

        public float ExperienceIntoLevel => _experienceIntoLevel;

        /// <summary>Experience the current level needs in total. 0 at the level cap.</summary>
        public float ExperienceForNextLevel => _curve == null || IsMaxLevel ? 0f : _curve.GetRequirement(Level);

        /// <summary>All experience gained since the start, including what was spent on levels.</summary>
        public float TotalExperience => _totalExperience;

        public bool IsMaxLevel => _curve != null && LevelCalculator.IsMaxLevel(_curve, Level);

        /// <summary>Progress through the current level, 0..1 (1 at the level cap).</summary>
        public float NormalizedProgress
        {
            get
            {
                float required = ExperienceForNextLevel;
                return required > 0f ? Mathf.Clamp01(_experienceIntoLevel / required) : 1f;
            }
        }

        public ExperienceCurveDefinition Curve => _curve;

        private void Awake()
        {
            EnsureInitialized();
            if (_curve == null)
            {
                Debug.LogError($"{nameof(CharacterExperience)} on '{name}' has no {nameof(ExperienceCurveDefinition)} assigned.", this);
            }
        }

        /// <summary>Adds experience and levels up as many times as it pays for. Returns the levels gained.</summary>
        public int AddExperience(float amount, GameObject source = null)
        {
            EnsureInitialized();
            if (_curve == null || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
            {
                return 0;
            }

            int previousLevel = _level;
            int gained = LevelCalculator.AddExperience(_curve, ref _level, ref _experienceIntoLevel, amount);
            _totalExperience += amount;

            ExperienceGained?.Invoke(amount, source);
            ExperienceChanged?.Invoke(_experienceIntoLevel, ExperienceForNextLevel);
            if (gained > 0)
            {
                LevelChanged?.Invoke(previousLevel, _level);
                LeveledUp?.Invoke(new LevelUpInfo(previousLevel, _level));
            }

            return gained;
        }

        /// <summary>Back to the starting level with no experience (new run, respawn into a fresh run).</summary>
        public void ResetProgress()
        {
            EnsureInitialized();
            int previousLevel = _level;
            _level = _startingLevel;
            _experienceIntoLevel = 0f;
            _totalExperience = 0f;

            ExperienceChanged?.Invoke(_experienceIntoLevel, ExperienceForNextLevel);
            if (previousLevel != _level)
            {
                LevelChanged?.Invoke(previousLevel, _level);
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _level = Mathf.Max(1, _startingLevel);
        }
    }
}
