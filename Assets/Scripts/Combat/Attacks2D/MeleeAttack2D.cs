using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Close-range 2D attack: after the wind-up the hit lands only if the target is still within range
    /// (plus a small tolerance), so stepping away during the telegraph dodges it.
    /// </summary>
    public class MeleeAttack2D : Attack2D
    {
        [Tooltip("Extra range allowed when the hit lands, so a target stepping back slightly still gets hit.")]
        [SerializeField, Min(0f)] private float _hitRangeTolerance = 0.2f;

        protected override bool Resolve(Transform target, IDamageable damageable)
        {
            if (!IsWithin(target, Range + _hitRangeTolerance))
            {
                return false;
            }

            return damageable.ApplyDamage(CreateDamage(target.position)) > 0f;
        }
    }
}
