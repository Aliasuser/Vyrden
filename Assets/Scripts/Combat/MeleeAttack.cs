using System;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Close-range attack with a wind-up and a cooldown. The hit lands after the wind-up only if the
    /// target is still in range and both sides are alive. Does not decide when to attack; AI calls
    /// <see cref="TryStartAttack"/>.
    /// </summary>
    public class MeleeAttack : MonoBehaviour, IAttack
    {
        private const float MaxHeightDifference = 2f;

        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField] private DamageType _damageType = DamageType.Generic;
        [SerializeField, Min(0.1f)] private float _range = 1.8f;
        [Tooltip("Extra range allowed when the hit lands, so a target stepping back slightly still gets hit.")]
        [SerializeField, Min(0f)] private float _hitRangeTolerance = 0.3f;
        [Tooltip("Seconds from the start of one attack until the next can start.")]
        [SerializeField, Min(0f)] private float _cooldown = 1.5f;
        [Tooltip("Seconds between starting the attack and the hit landing.")]
        [SerializeField, Min(0f)] private float _windup = 0.4f;
        [Tooltip("Attacking is blocked while this is dead. Defaults to the Health on this object or a parent.")]
        [SerializeField] private Health _ownerHealth;

        private Transform _target;
        private IDamageable _targetDamageable;
        private bool _isAttacking;
        private float _hitTime;
        private float _nextReadyTime;

        /// <summary>Raised when the wind-up starts.</summary>
        public event Action AttackStarted;

        /// <summary>Raised when the attack resolves or is cancelled. Arg: whether damage was dealt.</summary>
        public event Action<bool> AttackFinished;

        public float Range => _range;
        public bool IsAttacking => _isAttacking;
        public bool IsReady => Time.time >= _nextReadyTime;
        private bool IsOwnerAlive => _ownerHealth == null || _ownerHealth.IsAlive;

        private void Awake()
        {
            if (_ownerHealth == null)
            {
                _ownerHealth = GetComponentInParent<Health>();
            }
        }

        private void OnDisable()
        {
            Cancel();
        }

        /// <summary>Applies attack stats from data (e.g. an enemy definition).</summary>
        public void Configure(float damage, float range, float cooldown, float windup)
        {
            _damage = Mathf.Max(0f, damage);
            _range = Mathf.Max(0.1f, range);
            _cooldown = Mathf.Max(0f, cooldown);
            _windup = Mathf.Max(0f, windup);
        }

        public bool CanReach(Transform target)
        {
            return IsInRange(target);
        }

        public bool IsInRange(Transform target, float extraRange = 0f)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 offset = target.position - transform.position;
            if (Mathf.Abs(offset.y) > MaxHeightDifference)
            {
                return false;
            }

            offset.y = 0f;
            float range = _range + extraRange;
            return offset.sqrMagnitude <= range * range;
        }

        public bool TryStartAttack(Transform target)
        {
            if (!isActiveAndEnabled || _isAttacking || !IsReady || !IsOwnerAlive || !IsInRange(target))
            {
                return false;
            }

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            _target = target;
            _targetDamageable = damageable;
            _isAttacking = true;
            _hitTime = Time.time + _windup;
            _nextReadyTime = Time.time + _cooldown;
            AttackStarted?.Invoke();
            return true;
        }

        public void Cancel()
        {
            if (!_isAttacking)
            {
                return;
            }

            ClearAttack();
            AttackFinished?.Invoke(false);
        }

        private void Update()
        {
            if (!_isAttacking || Time.time < _hitTime)
            {
                return;
            }

            bool canHit = IsOwnerAlive && _target != null && _targetDamageable.IsAlive
                && IsInRange(_target, _hitRangeTolerance);

            float dealt = 0f;
            if (canHit)
            {
                DamageInfo damage = DamageCalculator.Calculate(_damage, 1f, _damageType, gameObject, _target.position);
                dealt = _targetDamageable.ApplyDamage(damage);
            }

            ClearAttack();
            AttackFinished?.Invoke(dealt > 0f);
        }

        private void ClearAttack()
        {
            _isAttacking = false;
            _target = null;
            _targetDamageable = null;
        }
    }
}
