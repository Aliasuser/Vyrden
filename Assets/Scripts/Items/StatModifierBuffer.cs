using System;

namespace Vyrden.Items
{
    /// <summary>Collects flat and percent modifiers per stat during a recalculation. Reused, no allocations.</summary>
    public sealed class StatModifierBuffer
    {
        private static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;

        private readonly float[] _flat = new float[StatCount];
        private readonly float[] _percent = new float[StatCount];

        public void Clear()
        {
            Array.Clear(_flat, 0, _flat.Length);
            Array.Clear(_percent, 0, _percent.Length);
        }

        public void Add(StatType stat, StatModifierType type, float value)
        {
            if (type == StatModifierType.Flat)
            {
                _flat[(int)stat] += value;
            }
            else
            {
                _percent[(int)stat] += value;
            }
        }

        public float Apply(StatType stat, float baseValue)
        {
            return (baseValue + _flat[(int)stat]) * (1f + _percent[(int)stat]);
        }
    }
}
