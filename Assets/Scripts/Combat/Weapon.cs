using System;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Runtime weapon: handles the trigger, cooldown and owner state, then lets the
    /// <see cref="WeaponDefinition"/> resolve the shot. Does not read input; something else
    /// (player input, AI) calls <see cref="SetTrigger"/>.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition _definition;
        [Tooltip("Where shots start. Defaults to this transform.")]
        [SerializeField] private Transform _muzzle;
        [Tooltip("Firing is blocked while this is dead. Defaults to the Health on a parent.")]
        [SerializeField] private Health _ownerHealth;

        private Transform _owner;
        private bool _triggerHeld;
        private bool _triggerPressed;
        private Ray _aimRay;
        private float _nextFireTime;
        private float _damageMultiplier = 1f;
        private float _fireRateMultiplier = 1f;

        /// <summary>Raised after every shot, hit or miss.</summary>
        public event Action<ShotResult> Fired;

        public WeaponDefinition Definition => _definition;
        public Transform Muzzle => _muzzle;
        public bool IsOwnerAlive => _ownerHealth == null || _ownerHealth.IsAlive;
        public bool IsReady => Time.time >= _nextFireTime;

        private void Awake()
        {
            if (_muzzle == null)
            {
                _muzzle = transform;
            }

            if (_ownerHealth == null)
            {
                _ownerHealth = GetComponentInParent<Health>();
            }

            _owner = _ownerHealth != null ? _ownerHealth.transform : transform.root;

            if (_definition == null)
            {
                Debug.LogError($"{nameof(Weapon)} on '{name}' has no {nameof(WeaponDefinition)} assigned.", this);
                enabled = false;
            }
        }

        /// <summary>Call every frame with the trigger state and where the shooter is aiming.</summary>
        public void SetTrigger(bool held, Ray aimRay)
        {
            if (held && !_triggerHeld)
            {
                _triggerPressed = true;
            }

            _triggerHeld = held;
            _aimRay = aimRay;
        }

        /// <summary>
        /// 2D version of <see cref="SetTrigger(bool, Ray)"/>: aims from the muzzle along
        /// <paramref name="aimDirection"/> on the XY plane. Call every frame.
        /// </summary>
        public void SetTrigger(bool held, Vector2 aimDirection)
        {
            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position;
            SetTrigger(held, new Ray(origin, new Vector3(aimDirection.x, aimDirection.y, 0f)));
        }

        /// <summary>Outgoing damage multiplier from items or buffs.</summary>
        public void SetDamageMultiplier(float multiplier)
        {
            _damageMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>Attack speed from items or buffs. 2 = twice as many shots per second.</summary>
        public void SetFireRateMultiplier(float multiplier)
        {
            _fireRateMultiplier = Mathf.Max(0.01f, multiplier);
        }

        public float FireRateMultiplier => _fireRateMultiplier;

        private void Update()
        {
            bool wantsToFire = _definition.IsAutomatic ? _triggerHeld : _triggerPressed;
            _triggerPressed = false;

            if (wantsToFire && IsReady && IsOwnerAlive)
            {
                Fire();
            }
        }

        private void Fire()
        {
            // Keep a steady cadence while held, but don't bank shots after a pause.
            float cooldown = _definition.FireCooldown / _fireRateMultiplier;
            if (Time.time - _nextFireTime > cooldown)
            {
                _nextFireTime = Time.time;
            }
            _nextFireTime += cooldown;

            var context = new ShotContext
            {
                AimRay = _aimRay,
                MuzzlePosition = _muzzle.position,
                Owner = _owner,
                DamageMultiplier = _damageMultiplier,
            };

            ShotResult result = _definition.Fire(context);
            Fired?.Invoke(result);
        }
    }
}
