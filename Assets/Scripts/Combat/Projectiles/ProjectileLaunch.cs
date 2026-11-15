using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>Everything a <see cref="Projectile2D"/> needs for one flight. Built by the weapon definition.</summary>
    public struct ProjectileLaunch
    {
        /// <summary>Where the shot starts on the XY plane (the shooter's aim origin). Checked for walls up to the spawn point.</summary>
        public Vector2 Origin;

        /// <summary>Unit travel direction on the XY plane.</summary>
        public Vector2 Direction;

        /// <summary>Distance from <see cref="Origin"/> where the projectile appears.</summary>
        public float SpawnOffset;

        public float Speed;
        public float Lifetime;

        /// <summary>Damage applied to the first living damageable hit; its point is set on impact.</summary>
        public DamageInfo Damage;

        /// <summary>Root of the shooter; its colliders are never hit.</summary>
        public Transform Owner;

        /// <summary>Physics layer for the projectile object, or -1 to keep the prefab's layer.</summary>
        public int Layer;

        /// <summary>Layers the projectile stops on (targets and obstacles).</summary>
        public int HitMask;
    }
}
