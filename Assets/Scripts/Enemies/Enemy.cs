using UnityEngine;
using Vyrden.Combat;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Root of an enemy. Applies its <see cref="EnemyDefinition"/> to the health and attack components
    /// so tuning lives in one data asset.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Health), typeof(TeamMember))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private MeleeAttack _attack;

        private Health _health;

        public EnemyDefinition Definition => _definition;
        public Health Health => _health != null ? _health : _health = GetComponent<Health>();
        public MeleeAttack Attack => _attack;

        private void Awake()
        {
            if (_attack == null)
            {
                _attack = GetComponentInChildren<MeleeAttack>();
            }

            if (_definition == null)
            {
                Debug.LogError($"{nameof(Enemy)} on '{name}' has no {nameof(EnemyDefinition)} assigned.", this);
                return;
            }

            Health.SetMaxHealth(_definition.MaxHealth);
            Health.ResetHealth();

            if (_attack != null)
            {
                _attack.Configure(_definition.AttackDamage, _definition.AttackRange,
                    _definition.AttackCooldown, _definition.AttackWindup);
            }
        }
    }
}
