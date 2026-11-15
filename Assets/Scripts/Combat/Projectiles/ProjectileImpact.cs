using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>What a projectile hit. Effects and UI read this; they never decide damage.</summary>
    public struct ProjectileImpact
    {
        public Projectile2D Projectile;
        public Vector2 Point;
        public Vector2 Normal;
        public Collider2D Collider;

        /// <summary>The shooter's root, if any.</summary>
        public Transform Owner;

        /// <summary>True when the hit object could take damage and was alive.</summary>
        public bool HitDamageable;
        public float DamageDealt;
        public bool Killed;
    }
}
