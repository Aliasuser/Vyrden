using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Root of an enemy. Applies its <see cref="EnemyDefinition"/> to the health and attack components
    /// so tuning lives in one data asset. The attack is any <see cref="IAttack"/> on the enemy (3D
    /// <see cref="MeleeAttack"/>, or the 2D melee / ranged attacks), so enemy types differ by composition.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Health), typeof(TeamMember))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private MeleeAttack _attack;

        private Health _health;
        private IAttack _attackBehaviour;
        private float _healthMultiplier = 1f;
        private float _damageMultiplier = 1f;

        public EnemyDefinition Definition => _definition;
        public Health Health => _health != null ? _health : _health = GetComponent<Health>();
        /// <summary>The 3D prototype's melee attack (null on 2D enemies).</summary>
        public MeleeAttack Attack => _attack;

        /// <summary>The enemy's attack, whatever kind it is. Used by the 2D brain.</summary>
        public IAttack AttackBehaviour => _attackBehaviour;
        public float HealthMultiplier => _healthMultiplier;
        public float DamageMultiplier => _damageMultiplier;

        private void Awake()
        {
            if (_attack == null)
            {
                _attack = GetComponentInChildren<MeleeAttack>();
            }

            _attackBehaviour = _attack != null ? _attack : GetComponentInChildren<IAttack>();

            if (_definition == null)
            {
                Debug.LogError($"{nameof(Enemy)} on '{name}' has no {nameof(EnemyDefinition)} assigned.", this);
                return;
            }

            ApplyDefinition();
        }

        /// <summary>
        /// Scales health and attack damage from the definition (difficulty, elites later). Meant for a freshly
        /// spawned enemy: health is refilled to the new maximum. Multipliers are not cumulative.
        /// </summary>
        public void ApplyStatMultipliers(float healthMultiplier, float damageMultiplier)
        {
            _healthMultiplier = Mathf.Max(0.01f, healthMultiplier);
            _damageMultiplier = Mathf.Max(0f, damageMultiplier);
            if (_definition != null && Health.IsAlive)
            {
                ApplyDefinition();
            }
        }

        private void ApplyDefinition()
        {
            Health.SetMaxHealth(_definition.MaxHealth * _healthMultiplier);
            Health.ResetHealth();

            if (_attackBehaviour != null)
            {
                _attackBehaviour.Configure(_definition.AttackDamage * _damageMultiplier, _definition.AttackRange,
                    _definition.AttackCooldown, _definition.AttackWindup);
            }
        }
    }
}
