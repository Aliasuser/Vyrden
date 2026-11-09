using System.Collections.Generic;
using UnityEngine;
using Vyrden.Stats;

namespace Vyrden.Items
{
    /// <summary>Changes one character stat, scaled by the number of copies held.</summary>
    [CreateAssetMenu(fileName = "StatModifierEffect", menuName = "Vyrden/Items/Stat Modifier Effect")]
    public class StatModifierEffect : ItemEffect
    {
        [SerializeField] private StatType _stat = StatType.MaxHealth;
        [SerializeField] private StatModifierType _modifierType = StatModifierType.Flat;
        [Tooltip("Flat: units per copy (e.g. 25 health). Percent types: fraction per copy (0.1 = +10%).")]
        [SerializeField] private float _valuePerStack = 1f;
        [SerializeField] private StackingRule _stacking = StackingRule.Linear;

        public StatType Stat => _stat;
        public StatModifierType ModifierType => _modifierType;
        public float ValuePerStack => _valuePerStack;
        public StackingRule Stacking => _stacking;

        public override void CollectStatModifiers(int stacks, List<StatModifier> output, object source)
        {
            if (stacks <= 0)
            {
                return;
            }

            float value = StackingCalculator.Evaluate(_stacking, _valuePerStack, stacks);
            output.Add(new StatModifier(_stat, _modifierType, value, source));
        }
    }
}
