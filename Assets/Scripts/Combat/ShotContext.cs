using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>Everything a weapon definition needs to resolve one shot.</summary>
    public struct ShotContext
    {
        /// <summary>Where the shooter is aiming (for the player: from the camera through the crosshair).</summary>
        public Ray AimRay;

        /// <summary>Where the shot visually and physically starts.</summary>
        public Vector3 MuzzlePosition;

        /// <summary>Root of the shooter. Its colliders are ignored and it is the damage instigator.</summary>
        public Transform Owner;

        /// <summary>Outgoing damage multiplier (items, buffs). 1 = unmodified.</summary>
        public float DamageMultiplier;
    }
}
