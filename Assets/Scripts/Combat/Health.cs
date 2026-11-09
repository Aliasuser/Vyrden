using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Health for any character (player or enemy). Knows nothing about UI or input:
    /// other systems call <see cref="ApplyDamage"/> / <see cref="Heal"/> and listen to the events.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float _maxHealth = 100f;

        private readonly List<IDamageModifier> _modifiers = new List<IDamageModifier>();
        private float _currentHealth;
        private bool _isDead;
        private bool _initialized;

        /// <summary>Raised whenever current or max health changes. Args: current, max.</summary>
        public event Action<float, float> HealthChanged;

        /// <summary>Raised after damage is applied. Args: final damage info, health actually lost.</summary>
        public event Action<DamageInfo, float> Damaged;

        /// <summary>Raised after healing. Arg: health actually restored.</summary>
        public event Action<float> Healed;

        /// <summary>Raised once when health reaches zero. Arg: the killing blow.</summary>
        public event Action<DamageInfo> Died;

        /// <summary>Raised when a dead character is brought back with <see cref="ResetHealth"/>.</summary>
        public event Action Revived;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return _currentHealth;
            }
        }
        public float NormalizedHealth => CurrentHealth / _maxHealth;
        public bool IsAlive => !_isDead;
        public bool IsDead => _isDead;

        private void Awake()
        {
            EnsureInitialized();

            foreach (IDamageModifier modifier in GetComponents<IDamageModifier>())
            {
                AddModifier(modifier);
            }
        }

        private void OnValidate()
        {
            _maxHealth = Mathf.Max(1f, _maxHealth);
        }

        public float ApplyDamage(in DamageInfo damage)
        {
            EnsureInitialized();

            if (_isDead || !IsValidAmount(damage.Amount))
            {
                return 0f;
            }

            DamageInfo finalDamage = damage;
            for (int i = 0; i < _modifiers.Count; i++)
            {
                _modifiers[i].ModifyIncomingDamage(ref finalDamage);
            }

            if (!IsValidAmount(finalDamage.Amount))
            {
                return 0f; // Fully absorbed.
            }

            float healthLost = Mathf.Min(finalDamage.Amount, _currentHealth);
            _currentHealth -= healthLost;

            Damaged?.Invoke(finalDamage, healthLost);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                _isDead = true;
                Died?.Invoke(finalDamage);
            }

            return healthLost;
        }

        /// <summary>Restores health up to the maximum. Does nothing while dead. Returns health actually restored.</summary>
        public float Heal(float amount)
        {
            EnsureInitialized();

            if (_isDead || !IsValidAmount(amount))
            {
                return 0f;
            }

            float healed = Mathf.Min(amount, _maxHealth - _currentHealth);
            if (healed <= 0f)
            {
                return 0f;
            }

            _currentHealth += healed;
            Healed?.Invoke(healed);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            return healed;
        }

        /// <summary>Changes max health (e.g. from items or level scaling). Current health is clamped to the new max.</summary>
        public void SetMaxHealth(float maxHealth)
        {
            EnsureInitialized();

            if (!IsValidAmount(maxHealth))
            {
                Debug.LogWarning($"{nameof(Health)} on '{name}': ignored invalid max health {maxHealth}.", this);
                return;
            }

            _maxHealth = maxHealth;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>Sets health back to full and clears the dead state (respawn, object pooling).</summary>
        public void ResetHealth()
        {
            EnsureInitialized();

            bool wasDead = _isDead;
            _isDead = false;
            _currentHealth = _maxHealth;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (wasDead)
            {
                Revived?.Invoke();
            }
        }

        public void AddModifier(IDamageModifier modifier)
        {
            if (modifier != null && !_modifiers.Contains(modifier))
            {
                _modifiers.Add(modifier);
                SortModifiers();
            }
        }

        public void RemoveModifier(IDamageModifier modifier)
        {
            _modifiers.Remove(modifier);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _currentHealth = _maxHealth;
        }

        private void SortModifiers()
        {
            _modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        private static bool IsValidAmount(float amount)
        {
            return amount > 0f && !float.IsNaN(amount) && !float.IsInfinity(amount);
        }
    }
}
