using System;
using UnityEngine;
using Vyrden.Stats;

namespace Vyrden.Progression
{
    /// <summary>One stat that grows with level: <see cref="ValuePerLevel"/> for every level above 1.</summary>
    [Serializable]
    public struct LevelStatGrowth
    {
        [SerializeField] private StatType _stat;
        [SerializeField] private StatModifierType _modifierType;
        [Tooltip("Flat: units per level (e.g. 15 health). Percent types: fraction per level (0.05 = +5%).")]
        [SerializeField] private float _valuePerLevel;

        public LevelStatGrowth(StatType stat, StatModifierType modifierType, float valuePerLevel)
        {
            _stat = stat;
            _modifierType = modifierType;
            _valuePerLevel = valuePerLevel;
        }

        public StatType Stat => _stat;
        public StatModifierType ModifierType => _modifierType;
        public float ValuePerLevel => _valuePerLevel;
    }
}
