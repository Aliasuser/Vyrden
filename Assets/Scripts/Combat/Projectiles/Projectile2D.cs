using System;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// A straight-flying projectile on the XY plane. Each physics step it circle-casts along the path it is
    /// about to travel, so it cannot pass through walls or targets at any speed. Stops on the first collider in
    /// its hit mask: a living <see cref="IDamageable"/> takes the launch damage, anything else (walls, rocks)
    /// just stops it. Expires after its lifetime. Works for any team; the launch decides what it can hit.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    [DisallowMultipleComponent]
    public class Projectile2D : MonoBehaviour
    {
        private const int MaxHits = 8;
        private const float LifetimeTolerance = 1e-4f;

        // Shared by all projectiles; casts run one at a time on the main thread.
        private static readonly RaycastHit2D[] Hits = new RaycastHit2D[MaxHits];

        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private ProjectileLaunch _launch;
        private ContactFilter2D _filter;
        private Vector2 _position;
        private float _age;
        private bool _inFlight;

        /// <summary>Raised when the projectile hits something, before it is removed.</summary>
        public event Action<ProjectileImpact> Impacted;

        /// <summary>Raised when the lifetime runs out without a hit.</summary>
        public event Action<Projectile2D> Expired;

        public Vector2 Direction => _launch.Direction;
        public Vector2 Velocity => _inFlight ? _launch.Direction * _launch.Speed : Vector2.zero;
        public Vector2 Position => _position;
        public float Age => _age;
        public bool IsInFlight => _inFlight;
        public Transform Owner => _launch.Owner;

        /// <summary>Prefab this instance came from; set by <see cref="ProjectilePool"/>.</summary>
        internal Projectile2D SourcePrefab { get; set; }

        /// <summary>World radius used for hit casts.</summary>
        public float Radius
        {
            get
            {
                Vector3 scale = transform.lossyScale;
                return _collider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();

            // Hits come from casts, not from the solver: the body never pushes or gets pushed.
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.gravityScale = 0f;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _collider.isTrigger = true;
        }

        /// <summary>Starts a flight. The instance may be inactive (pooled); it is activated here.</summary>
        public void Launch(in ProjectileLaunch launch)
        {
            _launch = launch;
            _launch.Direction = launch.Direction.sqrMagnitude > 0f ? launch.Direction.normalized : Vector2.down;
            _age = 0f;
            // Triggers count: trigger colliders on the Player / Enemy layers are body-sized hurtboxes, while
            // the solid collider only covers the feet. Other trigger layers are never in a hit mask.
            _filter = new ContactFilter2D { useLayerMask = true, layerMask = launch.HitMask, useTriggers = true };

            if (launch.Layer >= 0)
            {
                gameObject.layer = launch.Layer;
            }

            float angle = Mathf.Atan2(_launch.Direction.y, _launch.Direction.x) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(new Vector3(launch.Origin.x, launch.Origin.y, 0f), Quaternion.Euler(0f, 0f, angle));
            _position = launch.Origin;

            // The stretch from the shooter to the spawn point is checked too, so a shot fired with the
            // muzzle against (or past) a wall hits the wall instead of appearing on the other side.
            if (TrySweep(launch.Origin, _launch.SpawnOffset, out RaycastHit2D hit))
            {
                _inFlight = true;
                Impact(hit);
                return;
            }

            _position = launch.Origin + _launch.Direction * _launch.SpawnOffset;
            transform.position = new Vector3(_position.x, _position.y, 0f);
            gameObject.SetActive(true);
            _body.position = _position;
            _inFlight = true;
        }

        private void FixedUpdate()
        {
            if (!_inFlight)
            {
                return;
            }

            float stepTime = Mathf.Min(Time.fixedDeltaTime, _launch.Lifetime - _age);
            float step = _launch.Speed * stepTime;

            if (TrySweep(_position, step, out RaycastHit2D hit))
            {
                Impact(hit);
                return;
            }

            _position += _launch.Direction * step;
            _body.MovePosition(_position);
            _age += stepTime;

            // Tolerance for float drift, so a 0.8 s lifetime does not get an extra near-zero step.
            if (_age >= _launch.Lifetime - LifetimeTolerance)
            {
                _inFlight = false;
                Expired?.Invoke(this);
                Despawn();
            }
        }

        /// <summary>Closest valid hit along the path: not the owner, not a dead damageable.</summary>
        private bool TrySweep(Vector2 from, float distance, out RaycastHit2D closest)
        {
            closest = default;
            int count = Physics2D.CircleCast(from, Radius, _launch.Direction, _filter, Hits, Mathf.Max(distance, 0f));
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = Hits[i];
                if (_launch.Owner != null && hit.transform.IsChildOf(_launch.Owner))
                {
                    continue;
                }

                // Corpses do not stop shots.
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && !damageable.IsAlive)
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closest = hit;
                }
            }

            return closestDistance < float.MaxValue;
        }

        private void Impact(RaycastHit2D hit)
        {
            _position = hit.centroid;
            transform.position = new Vector3(_position.x, _position.y, 0f);

            var impact = new ProjectileImpact
            {
                Projectile = this,
                Point = hit.point,
                Normal = hit.normal,
                Collider = hit.collider,
                Owner = _launch.Owner,
            };

            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null && target.IsAlive)
            {
                DamageInfo damage = _launch.Damage;
                damage.Point = new Vector3(hit.point.x, hit.point.y, 0f);
                impact.HitDamageable = true;
                impact.DamageDealt = target.ApplyDamage(damage);
                impact.Killed = !target.IsAlive;
            }

            _inFlight = false;
            Impacted?.Invoke(impact);
            Despawn();
        }

        private void Despawn()
        {
            _inFlight = false;
            ProjectilePool.Release(this);
        }
    }
}
