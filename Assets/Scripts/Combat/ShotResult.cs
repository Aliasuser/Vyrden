using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>Outcome of one shot. Effects and UI read this; they never decide damage.</summary>
    public struct ShotResult
    {
        public Vector3 Origin;
        public Vector3 EndPoint;

        /// <summary>True when the shot hit any collider within range.</summary>
        public bool HitSomething;
        public Vector3 HitNormal;
        public Collider HitCollider;

        /// <summary>True when the hit object can take damage and was alive.</summary>
        public bool HitDamageable;
        public float DamageDealt;
        public bool Killed;
    }
}
