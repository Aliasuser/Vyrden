using System;

namespace Vyrden.Stats
{
    /// <summary>
    /// Accumulates modifiers per stat during one recalculation and computes final values.
    /// Reused between recalculations, so it never allocates.
    /// </summary>
    public sealed class StatModifierBuffer
    {
        public static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;

        private readonly float[] _flat = new float[StatCount];
        private readonly float[] _additive = new float[StatCount];
        private readonly float[] _multiplier = new float[StatCount];

        public StatModifierBuffer()
        {
            Clear();
        }

        public void Clear()
        {
            Array.Clear(_flat, 0, StatCount);
            Array.Clear(_additive, 0, StatCount);
            for (int i = 0; i < StatCount; i++)
            {
                _multiplier[i] = 1f;
            }
        }

        public void Add(in StatModifier modifier)
        {
            int stat = (int)modifier.Stat;
            switch (modifier.Type)
            {
                case StatModifierType.Flat:
                    _flat[stat] += modifier.Value;
                    break;
                case StatModifierType.AdditivePercent:
                    _additive[stat] += modifier.Value;
                    break;
                case StatModifierType.MultiplicativePercent:
                    _multiplier[stat] *= 1f + modifier.Value;
                    break;
            }
        }

        public float Apply(StatType stat, float baseValue)
        {
            int i = (int)stat;
            return (baseValue + _flat[i]) * (1f + _additive[i]) * _multiplier[i];
        }
    }
}
