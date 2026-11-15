using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// A weapon that fires <see cref="Projectile2D"/>s on the XY plane. Fire rate, damage and hold-to-fire come
    /// from <see cref="WeaponDefinition"/>; how the projectile flies comes from its <see cref="ProjectileDefinition"/>.
    /// The shooter's team picks the projectile's layer and targets, so enemies can use the same definitions.
    /// Hits happen later, so <see cref="Fire"/> only reports the launch; listen to
    /// <see cref="Projectile2D.Impacted"/> for hits.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Vyrden/Weapons/Projectile Weapon")]
    public class ProjectileWeaponDefinition : WeaponDefinition
    {
        [Header("Projectile")]
        [SerializeField] private ProjectileDefinition _projectile;
        [Tooltip("Distance from the muzzle where the projectile appears. The path up to it is still checked, so a shot fired against a wall hits the wall.")]
        [SerializeField, Min(0f)] private float _spawnOffset = 0.35f;

        public ProjectileDefinition Projectile => _projectile;
        public float SpawnOffset => _spawnOffset;

        public override ShotResult Fire(in ShotContext context)
        {
            var result = new ShotResult { Origin = context.MuzzlePosition, EndPoint = context.MuzzlePosition };

            Vector2 direction = new Vector2(context.AimRay.direction.x, context.AimRay.direction.y);
            if (_projectile == null || _projectile.Prefab == null || direction.sqrMagnitude < 1e-8f)
            {
                return result;
            }

            TeamMember team = context.Owner != null ? context.Owner.GetComponent<TeamMember>() : null;
            Team side = team != null ? team.Team : Team.Neutral;

            var launch = new ProjectileLaunch
            {
                Origin = new Vector2(context.MuzzlePosition.x, context.MuzzlePosition.y),
                Direction = direction.normalized,
                SpawnOffset = _spawnOffset,
                Speed = _projectile.Speed,
                Lifetime = _projectile.Lifetime,
                Damage = CreateDamage(context, context.MuzzlePosition),
                Owner = context.Owner,
                Layer = ProjectileTargeting.GetLayer(side),
                HitMask = ProjectileTargeting.GetHitMask(side),
            };

            ProjectilePool.Get(_projectile.Prefab).Launch(launch);
            return result;
        }
    }
}
