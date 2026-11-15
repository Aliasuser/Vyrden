using Vyrden.Core;

namespace Vyrden.Combat
{
    /// <summary>
    /// Which layer a projectile lives on and what it can hit, from the shooter's team. This is what makes
    /// the same projectile code work for player and enemy shots.
    /// </summary>
    public static class ProjectileTargeting
    {
        private const int NeutralHitMask = PhysicsLayers.PlayerMask | PhysicsLayers.EnemyMask | PhysicsLayers.ObstacleMask;

        /// <summary>Physics layer for a projectile fired by <paramref name="team"/>, or -1 to keep the prefab's layer.</summary>
        public static int GetLayer(Team team)
        {
            switch (team)
            {
                case Team.Player: return PhysicsLayers.PlayerProjectile;
                case Team.Enemy: return PhysicsLayers.EnemyProjectile;
                default: return -1;
            }
        }

        /// <summary>Layers a projectile fired by <paramref name="team"/> stops on.</summary>
        public static int GetHitMask(Team team)
        {
            switch (team)
            {
                case Team.Player: return PhysicsLayers.PlayerProjectileTargets;
                case Team.Enemy: return PhysicsLayers.EnemyProjectileTargets;
                default: return NeutralHitMask;
            }
        }
    }
}
