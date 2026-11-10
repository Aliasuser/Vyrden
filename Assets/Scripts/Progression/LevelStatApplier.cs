using System.Collections.Generic;
using UnityEngine;
using Vyrden.Stats;

namespace Vyrden.Progression
{
    /// <summary>
    /// Bridges level and stats: whenever the level changes, rebuilds the level modifiers from scratch and
    /// hands them to <see cref="CharacterStats"/> as one source, so repeated level-ups never stack twice.
    /// </summary>
    [RequireComponent(typeof(CharacterExperience))]
    public class LevelStatApplier : MonoBehaviour
    {
        [SerializeField] private LevelStatGrowthDefinition _growth;
        [Tooltip("Defaults to the CharacterStats on this object.")]
        [SerializeField] private CharacterStats _stats;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private CharacterExperience _experience;

        private void Awake()
        {
            _experience = GetComponent<CharacterExperience>();
            if (_stats == null)
            {
                _stats = GetComponent<CharacterStats>();
            }

            if (_stats == null || _growth == null)
            {
                Debug.LogError($"{nameof(LevelStatApplier)} on '{name}' needs a {nameof(CharacterStats)} and a {nameof(LevelStatGrowthDefinition)}.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _experience.LevelChanged += OnLevelChanged;
            Apply();
        }

        private void OnDisable()
        {
            _experience.LevelChanged -= OnLevelChanged;
            if (_stats != null)
            {
                _stats.RemoveModifiersFromSource(this);
            }
        }

        private void OnLevelChanged(int previousLevel, int newLevel)
        {
            Apply();
        }

        public void Apply()
        {
            _modifiers.Clear();
            _growth.CollectStatModifiers(_experience.Level, _modifiers, this);
            _stats.ReplaceModifiers(this, _modifiers);
        }
    }
}
