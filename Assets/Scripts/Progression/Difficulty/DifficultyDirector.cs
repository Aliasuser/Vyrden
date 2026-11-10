using System;
using UnityEngine;
using Vyrden.Core;

namespace Vyrden.Progression
{
    /// <summary>
    /// The single runtime owner of difficulty: reads the run time, evaluates <see cref="DifficultySettings"/>
    /// and publishes the result. Systems that scale with difficulty (spawning, enemy stats) read
    /// <see cref="Current"/> instead of doing their own math. The run time stops while paused or after the
    /// run ends, and so does difficulty.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public class DifficultyDirector : MonoBehaviour
    {
        [SerializeField] private DifficultySettings _settings;
        [SerializeField] private RunTimer _runTimer;

        private DifficultyState _current = DifficultyState.Baseline;

        /// <summary>Raised when the coefficient moves into another tier. Args: previous index, new index.</summary>
        public event Action<int, int> TierChanged;

        public DifficultySettings Settings => _settings;
        public RunTimer RunTimer => _runTimer;
        public DifficultyState Current => _current;
        public float Coefficient => _current.Coefficient;

        /// <summary>Name of the current tier, or empty when none applies.</summary>
        public string CurrentTierName
        {
            get
            {
                DifficultyTier[] tiers = _settings != null ? _settings.Tiers : null;
                int index = _current.TierIndex;
                return tiers != null && index >= 0 && index < tiers.Length ? tiers[index].Name : string.Empty;
            }
        }

        private void Awake()
        {
            if (_settings == null || _runTimer == null)
            {
                Debug.LogError($"{nameof(DifficultyDirector)} on '{name}' needs {nameof(DifficultySettings)} and a {nameof(RunTimer)}.", this);
                enabled = false;
                return;
            }

            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        /// <summary>Re-evaluates difficulty from the current run time.</summary>
        public void Refresh()
        {
            if (_settings == null || _runTimer == null)
            {
                return;
            }

            int previousTier = _current.TierIndex;
            _current = DifficultyCalculator.Evaluate(_settings, _runTimer.ElapsedTime);
            if (_current.TierIndex != previousTier)
            {
                TierChanged?.Invoke(previousTier, _current.TierIndex);
            }
        }
    }
}
