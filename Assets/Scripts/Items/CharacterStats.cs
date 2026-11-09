using System.Collections.Generic;
using UnityEngine;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Items
{
    /// <summary>
    /// Turns held items into stat values and pushes them to the components that use them
    /// (Health, CharacterMotor, Weapon). Recalculates whenever the inventory changes.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class CharacterStats : MonoBehaviour
    {
        [Tooltip("Defaults to the Health on this object.")]
        [SerializeField] private Health _health;
        [Tooltip("Defaults to the CharacterMotor on this object.")]
        [SerializeField] private CharacterMotor _motor;
        [Tooltip("Defaults to the first Weapon in children.")]
        [SerializeField] private Weapon _weapon;

        private readonly StatModifierBuffer _buffer = new StatModifierBuffer();
        private Inventory _inventory;
        private float _baseMaxHealth;
        private float _maxHealth;
        private float _moveSpeedMultiplier = 1f;
        private float _damageMultiplier = 1f;

        public float MaxHealth => _maxHealth;
        public float MoveSpeedMultiplier => _moveSpeedMultiplier;
        public float DamageMultiplier => _damageMultiplier;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
            if (_health == null)
            {
                _health = GetComponent<Health>();
            }

            if (_motor == null)
            {
                _motor = GetComponent<CharacterMotor>();
            }

            if (_weapon == null)
            {
                _weapon = GetComponentInChildren<Weapon>();
            }

            _baseMaxHealth = _health != null ? _health.MaxHealth : 0f;
            _maxHealth = _baseMaxHealth;
        }

        private void OnEnable()
        {
            _inventory.Changed += Recalculate;
            Recalculate();
        }

        private void OnDisable()
        {
            _inventory.Changed -= Recalculate;
        }

        public float GetStat(StatType stat)
        {
            switch (stat)
            {
                case StatType.MaxHealth:
                    return _maxHealth;
                case StatType.MoveSpeed:
                    return _moveSpeedMultiplier;
                case StatType.Damage:
                    return _damageMultiplier;
                default:
                    return 0f;
            }
        }

        public void Recalculate()
        {
            _buffer.Clear();
            IReadOnlyList<ItemStack> items = _inventory.Items;
            for (int i = 0; i < items.Count; i++)
            {
                ItemStack stack = items[i];
                IReadOnlyList<ItemEffect> effects = stack.Item.Effects;
                for (int e = 0; e < effects.Count; e++)
                {
                    if (effects[e] != null)
                    {
                        effects[e].CollectStatModifiers(stack.Count, _buffer);
                    }
                }
            }

            _maxHealth = Mathf.Max(1f, _buffer.Apply(StatType.MaxHealth, _baseMaxHealth));
            _moveSpeedMultiplier = Mathf.Max(0f, _buffer.Apply(StatType.MoveSpeed, 1f));
            _damageMultiplier = Mathf.Max(0f, _buffer.Apply(StatType.Damage, 1f));

            ApplyMaxHealth();

            if (_motor != null)
            {
                _motor.SetSpeedMultiplier(_moveSpeedMultiplier);
            }

            if (_weapon != null)
            {
                _weapon.SetDamageMultiplier(_damageMultiplier);
            }
        }

        private void ApplyMaxHealth()
        {
            if (_health == null || Mathf.Approximately(_health.MaxHealth, _maxHealth))
            {
                return;
            }

            // Gaining max health also grants that much current health; losing it only clamps.
            float gained = _maxHealth - _health.MaxHealth;
            _health.SetMaxHealth(_maxHealth);
            if (gained > 0f)
            {
                _health.Heal(gained);
            }
        }
    }
}
