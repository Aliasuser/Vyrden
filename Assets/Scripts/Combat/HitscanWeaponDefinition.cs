using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Instant-hit weapon. Finds the aim point along the aim ray, then traces from the muzzle to it,
    /// so anything between the weapon and the target blocks the shot.
    /// </summary>
    [CreateAssetMenu(fileName = "HitscanWeapon", menuName = "Vyrden/Weapons/Hitscan Weapon")]
    public class HitscanWeaponDefinition : WeaponDefinition
    {
        private const int MaxHits = 16;

        [Header("Hitscan")]
        [SerializeField, Min(0.1f)] private float _range = 100f;
        [SerializeField] private LayerMask _hitLayers = ~0;

        // Shared by all hitscan weapons; shots are resolved one at a time on the main thread.
        private static readonly RaycastHit[] Hits = new RaycastHit[MaxHits];

        public float Range => _range;
        public LayerMask HitLayers => _hitLayers;

        public override ShotResult Fire(in ShotContext context)
        {
            Vector3 aimPoint = FindAimPoint(context);
            Vector3 origin = context.MuzzlePosition;
            Vector3 toAim = aimPoint - origin;
            Vector3 direction = toAim.sqrMagnitude > 0.0001f ? toAim.normalized : context.AimRay.direction;

            var result = new ShotResult
            {
                Origin = origin,
                EndPoint = origin + direction * _range,
            };

            if (!TryRaycast(origin, direction, _range, context.Owner, out RaycastHit hit))
            {
                return result;
            }

            result.HitSomething = true;
            result.EndPoint = hit.point;
            result.HitNormal = hit.normal;
            result.HitCollider = hit.collider;

            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null && target.IsAlive)
            {
                result.HitDamageable = true;
                result.DamageDealt = target.ApplyDamage(CreateDamage(context, hit.point));
                result.Killed = !target.IsAlive;
            }

            return result;
        }

        private Vector3 FindAimPoint(in ShotContext context)
        {
            Ray ray = context.AimRay;

            // Start level with the muzzle so objects between the camera and the character are not targeted.
            float skip = Mathf.Max(0f, Vector3.Dot(context.MuzzlePosition - ray.origin, ray.direction));
            Vector3 start = ray.origin + ray.direction * skip;

            return TryRaycast(start, ray.direction, _range, context.Owner, out RaycastHit hit)
                ? hit.point
                : start + ray.direction * _range;
        }

        private bool TryRaycast(Vector3 origin, Vector3 direction, float distance, Transform owner, out RaycastHit closest)
        {
            int count = Physics.RaycastNonAlloc(origin, direction, Hits, distance, _hitLayers, QueryTriggerInteraction.Ignore);

            closest = default;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Hits[i];
                if (owner != null && hit.transform.IsChildOf(owner))
                {
                    continue; // Never hit ourselves.
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closest = hit;
                }
            }

            return closestDistance < float.MaxValue;
        }
    }
}
