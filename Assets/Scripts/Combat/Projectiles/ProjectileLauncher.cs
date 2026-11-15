using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// One place that turns "fire this projectile from here, this way" into a pooled, team-aware launch.
    /// Used by projectile weapons and ranged enemy attacks.
    /// </summary>
    public static class ProjectileLauncher
    {
        /// <summary>
        /// Launches a pooled projectile. The owner's <see cref="TeamMember"/> team picks its layer and targets.
        /// Returns null if the definition has no prefab or the direction is zero.
        /// </summary>
        public static Projectile2D Launch(ProjectileDefinition projectile, Vector2 origin, Vector2 direction,
            float spawnOffset, in DamageInfo damage, Transform owner)
        {
            if (projectile == null || projectile.Prefab == null || direction.sqrMagnitude < 1e-8f)
            {
                return null;
            }

            TeamMember member = owner != null ? owner.GetComponent<TeamMember>() : null;
            Team team = member != null ? member.Team : Team.Neutral;

            var launch = new ProjectileLaunch
            {
                Origin = origin,
                Direction = direction.normalized,
                SpawnOffset = spawnOffset,
                Speed = projectile.Speed,
                Lifetime = projectile.Lifetime,
                Damage = damage,
                Owner = owner,
                Layer = ProjectileTargeting.GetLayer(team),
                HitMask = ProjectileTargeting.GetHitMask(team),
            };

            Projectile2D instance = ProjectilePool.Get(projectile.Prefab);
            instance.Launch(launch);
            return instance;
        }
    }
}
