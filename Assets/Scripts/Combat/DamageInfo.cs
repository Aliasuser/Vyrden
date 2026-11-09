using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Describes one hit. Passed through damage modifiers before it reaches health.
    /// </summary>
    public struct DamageInfo
    {
        public float Amount;
        public DamageType Type;

        /// <summary>Who caused the damage (character, trap...). Can be null.</summary>
        public GameObject Instigator;

        /// <summary>World position of the hit, used later for hit effects and knockback.</summary>
        public Vector3 Point;

        public DamageInfo(float amount, GameObject instigator = null, DamageType type = DamageType.Generic, Vector3 point = default)
        {
            Amount = amount;
            Type = type;
            Instigator = instigator;
            Point = point;
        }
    }
}
