namespace Vyrden.Core
{
    /// <summary>
    /// Physics layer indices for 2D gameplay, matching Project Settings → Tags and Layers. Use these and the
    /// masks instead of layer name strings. Which layers collide is set in the Physics 2D collision matrix
    /// (see ARCHITECTURE.md, 2D foundation).
    /// </summary>
    public static class PhysicsLayers
    {
        public const int Default = 0;
        public const int Player = 6;
        public const int Enemy = 7;
        public const int PlayerProjectile = 8;
        public const int EnemyProjectile = 9;

        /// <summary>Walls, rocks and anything else that blocks movement and projectiles.</summary>
        public const int Obstacle = 10;

        /// <summary>World pickups (items, currency); collected by the player, pushed out of obstacles.</summary>
        public const int Pickup = 11;

        /// <summary>Trigger areas of objects the player can use (chests, shops, exits).</summary>
        public const int Interactable = 12;

        /// <summary>Room and door triggers that only the player activates.</summary>
        public const int RoomTrigger = 13;

        public const int PlayerMask = 1 << Player;
        public const int EnemyMask = 1 << Enemy;
        public const int ObstacleMask = 1 << Obstacle;
        public const int PickupMask = 1 << Pickup;
        public const int InteractableMask = 1 << Interactable;
        public const int RoomTriggerMask = 1 << RoomTrigger;

        /// <summary>What player projectiles can hit.</summary>
        public const int PlayerProjectileTargets = EnemyMask | ObstacleMask;

        /// <summary>What enemy projectiles can hit.</summary>
        public const int EnemyProjectileTargets = PlayerMask | ObstacleMask;
    }
}
