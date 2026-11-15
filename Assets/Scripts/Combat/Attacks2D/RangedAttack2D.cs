using UnityEngine;
using Vyrden.Core;

namespace Vyrden.Combat
{
    /// <summary>
    /// Ranged 2D attack: needs a clear line past obstacles to start, then after the wind-up fires a pooled
    /// <see cref="Projectile2D"/> at where the target is now. The projectile does the damage (on the
    /// owner's team, so it never hits allies). Fires from this transform's position.
    /// </summary>
    public class RangedAttack2D : Attack2D
    {
        [SerializeField] private ProjectileDefinition _projectile;
        [Tooltip("Distance from this transform where the projectile appears (the path up to it is still checked).")]
        [SerializeField, Min(0f)] private float _spawnOffset = 0.3f;

        private float _shotRadius;

        public ProjectileDefinition Projectile => _projectile;

        private Vector2 Origin => new Vector2(transform.position.x, transform.position.y);

        protected override void Awake()
        {
            base.Awake();
            if (_projectile == null || _projectile.Prefab == null)
            {
                Debug.LogError($"{nameof(RangedAttack2D)} on '{name}' needs a {nameof(ProjectileDefinition)} with a prefab.", this);
                enabled = false;
                return;
            }

            // Line of sight is checked with the shot's width, so a shot never clips a corner the thin line missed.
            Transform shot = _projectile.Prefab.transform;
            CircleCollider2D shotCollider = shot.GetComponent<CircleCollider2D>();
            float scale = Mathf.Max(Mathf.Abs(shot.localScale.x), Mathf.Abs(shot.localScale.y));
            _shotRadius = shotCollider != null ? shotCollider.radius * scale : 0f;
        }

        public override bool CanReach(Transform target)
        {
            return base.CanReach(target) && HasLineOfSight(target);
        }

        /// <summary>True when no obstacle blocks a shot-wide path from the muzzle to the target.</summary>
        public bool HasLineOfSight(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Vector2 toTarget = GetAimPoint(target) - Origin;
            float distance = toTarget.magnitude;
            if (distance < 1e-4f)
            {
                return true;
            }

            return !Physics2D.CircleCast(Origin, _shotRadius, toTarget / distance, distance, PhysicsLayers.ObstacleMask);
        }

        protected override bool Resolve(Transform target, IDamageable damageable)
        {
            Vector2 toTarget = GetAimPoint(target) - Origin;
            Vector2 direction = toTarget.sqrMagnitude > 1e-6f ? toTarget.normalized : AttackDirection;
            Projectile2D shot = ProjectileLauncher.Launch(_projectile, Origin, direction, _spawnOffset, CreateDamage(Origin), Owner);
            return shot != null;
        }

        /// <summary>
        /// Where to aim at a target: the centre of its main collider (gameplay shape, not its sprite),
        /// or its pivot if it has none.
        /// </summary>
        private static Vector2 GetAimPoint(Transform target)
        {
            Collider2D body = target.GetComponent<Collider2D>();
            Vector3 point = body != null ? body.bounds.center : target.position;
            return new Vector2(point.x, point.y);
        }
    }
}
