using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Turns attack stats into a <see cref="DamageInfo"/>. Health only receives the result, so crits,
    /// item bonuses and scaling can be added here without touching health or weapons.
    /// </summary>
    public static class DamageCalculator
    {
        public static DamageInfo Calculate(float baseDamage, float damageMultiplier, DamageType type,
            GameObject instigator, Vector3 hitPoint)
        {
            float amount = Mathf.Max(0f, baseDamage) * Mathf.Max(0f, damageMultiplier);
            return new DamageInfo(amount, instigator, type, hitPoint);
        }
    }
}
