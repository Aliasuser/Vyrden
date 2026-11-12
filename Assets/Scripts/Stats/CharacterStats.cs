using System;
using System.Collections.Generic;
using UnityEngine;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Stats
{
    /// <summary>
    /// The single place a character's stats are calculated. Holds base values and a list of modifiers
    /// from any number of sources (items, buffs, difficulty...). Every change recomputes all stats from
    /// base + current modifiers, so nothing accumulates, then pushes the results to the components that
    /// use them (Health, CharacterMotor, Weapon, ArmorDamageModifier).
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        private const float MinAttackSpeed = 0.05f;

        [Tooltip("Starting values. When empty: max health from Health, multipliers 1, armor 0.")]
        [SerializeField] private CharacterBaseStats _baseStats;
        [Tooltip("Defaults to the Health on this object.")]
        [SerializeField] private Health _health;
        [Tooltip("3D prototype motor. When empty, the first IMovementSpeedReceiver on this object (e.g. CharacterMotor2D) gets the move speed.")]
        [SerializeField] private CharacterMotor _motor;
        [Tooltip("Defaults to the first Weapon in children.")]
        [SerializeField] private Weapon _weapon;
        [Tooltip("Defaults to the ArmorDamageModifier on this object.")]
        [SerializeField] private ArmorDamageModifier _armor;

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private IMovementSpeedReceiver _speedReceiver;
        private readonly StatModifierBuffer _buffer = new StatModifierBuffer();
        private readonly float[] _baseValues = new float[StatModifierBuffer.StatCount];
        private readonly float[] _values = new float[StatModifierBuffer.StatCount];
        private bool _initialized;

        /// <summary>Raised for each stat whose final value changed. Args: stat, new value.</summary>
        public event Action<StatType, float> StatChanged;

        public IReadOnlyList<StatModifier> Modifiers => _modifiers;

        private void Awake()
        {
            EnsureInitialized();
        }

        public float GetValue(StatType stat)
        {
            EnsureInitialized();
            return _values[(int)stat];
        }

        public float GetBaseValue(StatType stat)
        {
            EnsureInitialized();
            return _baseValues[(int)stat];
        }

        public void AddModifier(in StatModifier modifier)
        {
            EnsureInitialized();
            _modifiers.Add(modifier);
            Recalculate();
        }

        /// <summary>Removes every modifier added by <paramref name="source"/>. Returns how many were removed.</summary>
        public int RemoveModifiersFromSource(object source)
        {
            EnsureInitialized();
            int removed = RemoveWithoutRecalculating(source);
            if (removed > 0)
            {
                Recalculate();
            }

            return removed;
        }

        /// <summary>
        /// Replaces all of <paramref name="source"/>'s modifiers with <paramref name="modifiers"/> in one
        /// recalculation. Modifiers are re-tagged with <paramref name="source"/>.
        /// </summary>
        public void ReplaceModifiers(object source, IReadOnlyList<StatModifier> modifiers)
        {
            EnsureInitialized();
            RemoveWithoutRecalculating(source);
            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier m = modifiers[i];
                _modifiers.Add(new StatModifier(m.Stat, m.Type, m.Value, source));
            }

            Recalculate();
        }

        /// <summary>Recomputes every stat from base values and the current modifiers.</summary>
        public void Recalculate()
        {
            EnsureInitialized();

            _buffer.Clear();
            for (int i = 0; i < _modifiers.Count; i++)
            {
                _buffer.Add(_modifiers[i]);
            }

            for (int i = 0; i < StatModifierBuffer.StatCount; i++)
            {
                var stat = (StatType)i;
                float value = Clamp(stat, _buffer.Apply(stat, _baseValues[i]));
                if (!Mathf.Approximately(value, _values[i]))
                {
                    _values[i] = value;
                    StatChanged?.Invoke(stat, value);
                }
            }

            ApplyToComponents();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            if (_health == null)
            {
                _health = GetComponent<Health>();
            }

            if (_motor == null)
            {
                _motor = GetComponent<CharacterMotor>();
            }

            _speedReceiver = _motor != null ? _motor : GetComponent<IMovementSpeedReceiver>();

            if (_weapon == null)
            {
                _weapon = GetComponentInChildren<Weapon>();
            }

            if (_armor == null)
            {
                _armor = GetComponent<ArmorDamageModifier>();
            }

            for (int i = 0; i < StatModifierBuffer.StatCount; i++)
            {
                var stat = (StatType)i;
                _baseValues[i] = _baseStats != null ? _baseStats.Get(stat) : DefaultBase(stat);
                _values[i] = _baseValues[i];
            }

            ApplyToComponents();
        }

        private float DefaultBase(StatType stat)
        {
            switch (stat)
            {
                case StatType.MaxHealth:
                    return _health != null ? _health.MaxHealth : 100f;
                case StatType.Armor:
                    return 0f;
                default:
                    return 1f;
            }
        }

        private static float Clamp(StatType stat, float value)
        {
            switch (stat)
            {
                case StatType.MaxHealth:
                    return Mathf.Max(1f, value);
                case StatType.AttackSpeed:
                    return Mathf.Max(MinAttackSpeed, value);
                case StatType.Armor:
                    return value; // Negative armor is allowed and increases damage taken.
                default:
                    return Mathf.Max(0f, value);
            }
        }

        private int RemoveWithoutRecalculating(object source)
        {
            int removed = 0;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (Equals(_modifiers[i].Source, source))
                {
                    _modifiers.RemoveAt(i);
                    removed++;
                }
            }

            return removed;
        }

        private void ApplyToComponents()
        {
            if (_health != null)
            {
                float maxHealth = _values[(int)StatType.MaxHealth];
                float gained = maxHealth - _health.MaxHealth;
                if (!Mathf.Approximately(gained, 0f))
                {
                    // Gaining max health also grants that much current health; losing it only clamps.
                    _health.SetMaxHealth(maxHealth);
                    if (gained > 0f)
                    {
                        _health.Heal(gained);
                    }
                }
            }

            if (_speedReceiver != null)
            {
                _speedReceiver.SetSpeedMultiplier(_values[(int)StatType.MoveSpeed]);
            }

            if (_weapon != null)
            {
                _weapon.SetDamageMultiplier(_values[(int)StatType.Damage]);
                _weapon.SetFireRateMultiplier(_values[(int)StatType.AttackSpeed]);
            }

            if (_armor != null)
            {
                _armor.SetArmor(_values[(int)StatType.Armor]);
            }
        }
    }
}
