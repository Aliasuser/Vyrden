using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Base data for every weapon. Subclasses decide how a shot is resolved
    /// (hitscan now; projectile, melee or spread later).
    /// </summary>
    public abstract class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Weapon";

        [Header("Damage")]
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField] private DamageType _damageType = DamageType.Generic;

        [Header("Firing")]
        [Tooltip("Shots per second. The cooldown between shots is 1 / fire rate.")]
        [SerializeField, Min(0.01f)] private float _fireRate = 8f;
        [Tooltip("Keep firing while the trigger is held. Off = one shot per press.")]
        [SerializeField] private bool _isAutomatic = true;

        public string DisplayName => _displayName;
        public float Damage => _damage;
        public DamageType DamageType => _damageType;
        public float FireRate => _fireRate;
        public float FireCooldown => 1f / _fireRate;
        public bool IsAutomatic => _isAutomatic;

        /// <summary>Resolves one shot, applies damage and reports what happened.</summary>
        public abstract ShotResult Fire(in ShotContext context);

        /// <summary>Damage for one hit, built through the shared calculator.</summary>
        protected DamageInfo CreateDamage(in ShotContext context, Vector3 hitPoint)
        {
            GameObject instigator = context.Owner != null ? context.Owner.gameObject : null;
            return DamageCalculator.Calculate(_damage, context.DamageMultiplier, _damageType, instigator, hitPoint);
        }
    }
}
