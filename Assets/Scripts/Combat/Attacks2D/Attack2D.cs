using System;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Shared timing for 2D attacks: wind-up, cooldown, owner-alive checks and events. Range is measured on
    /// the XY plane between the owner's and the target's pivots (their feet). Subclasses decide whether a
    /// target is reachable and what happens when the wind-up ends.
    /// </summary>
    public abstract class Attack2D : MonoBehaviour, IAttack
    {
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField] private DamageType _damageType = DamageType.Generic;
        [SerializeField, Min(0.1f)] private float _range = 1f;
        [Tooltip("Seconds from the start of one attack until the next can start.")]
        [SerializeField, Min(0f)] private float _cooldown = 1.5f;
        [Tooltip("Seconds between starting the attack and it resolving (the telegraph).")]
        [SerializeField, Min(0f)] private float _windup = 0.4f;
        [Tooltip("Attacking is blocked while this is dead. Defaults to the Health on this object or a parent.")]
        [SerializeField] private Health _ownerHealth;

        private Transform _target;
        private IDamageable _targetDamageable;
        private bool _isAttacking;
        private float _resolveTime;
        private float _nextReadyTime;
        private Vector2 _attackDirection = Vector2.down;

        public event Action AttackStarted;
        public event Action<bool> AttackFinished;

        public float Damage => _damage;
        public DamageType DamageType => _damageType;
        public float Range => _range;
        public float Cooldown => _cooldown;
        public float Windup => _windup;
        public bool IsAttacking => _isAttacking;
        public bool IsReady => Time.time >= _nextReadyTime;

        /// <summary>Direction to the target when the current or last attack started (for facing / animation).</summary>
        public Vector2 AttackDirection => _attackDirection;

        /// <summary>Root of the attacker; damage instigator and range origin.</summary>
        protected Transform Owner { get; private set; }

        protected bool IsOwnerAlive => _ownerHealth == null || _ownerHealth.IsAlive;
        protected Vector2 OwnerPosition => new Vector2(Owner.position.x, Owner.position.y);

        protected virtual void Awake()
        {
            if (_ownerHealth == null)
            {
                _ownerHealth = GetComponentInParent<Health>();
            }

            Owner = _ownerHealth != null ? _ownerHealth.transform : transform.root;
        }

        protected virtual void OnDisable()
        {
            Cancel();
        }

        public void Configure(float damage, float range, float cooldown, float windup)
        {
            _damage = Mathf.Max(0f, damage);
            _range = Mathf.Max(0.1f, range);
            _cooldown = Mathf.Max(0f, cooldown);
            _windup = Mathf.Max(0f, windup);
        }

        /// <summary>Within range on the XY plane. Subclasses add conditions (line of sight).</summary>
        public virtual bool CanReach(Transform target)
        {
            return target != null && IsWithin(target, _range);
        }

        public bool TryStartAttack(Transform target)
        {
            if (!isActiveAndEnabled || _isAttacking || !IsReady || !IsOwnerAlive || !CanReach(target))
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
            _resolveTime = Time.time + _windup;
            _nextReadyTime = Time.time + _cooldown;
            _attackDirection = DirectionTo(target);
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
            if (!_isAttacking || Time.time < _resolveTime)
            {
                return;
            }

            bool landed = IsOwnerAlive && _target != null && _targetDamageable.IsAlive && Resolve(_target, _targetDamageable);
            ClearAttack();
            AttackFinished?.Invoke(landed);
        }

        /// <summary>Called when the wind-up ends with both sides alive. Returns whether the attack landed.</summary>
        protected abstract bool Resolve(Transform target, IDamageable damageable);

        protected bool IsWithin(Transform target, float range)
        {
            Vector2 offset = new Vector2(target.position.x, target.position.y) - OwnerPosition;
            return offset.sqrMagnitude <= range * range;
        }

        protected Vector2 DirectionTo(Transform target)
        {
            Vector2 offset = new Vector2(target.position.x, target.position.y) - OwnerPosition;
            return offset.sqrMagnitude > 1e-6f ? offset.normalized : _attackDirection;
        }

        protected DamageInfo CreateDamage(Vector2 point)
        {
            GameObject instigator = Owner != null ? Owner.gameObject : gameObject;
            return DamageCalculator.Calculate(_damage, 1f, _damageType, instigator, new Vector3(point.x, point.y, 0f));
        }

        private void ClearAttack()
        {
            _isAttacking = false;
            _target = null;
            _targetDamageable = null;
        }
    }
}
