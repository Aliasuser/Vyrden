namespace Vyrden.Core
{
    /// <summary>
    /// Sprite sorting layers, back to front, matching Project Settings → Tags and Layers. Inside
    /// <see cref="Entities"/> objects are ordered by their Y position (lower on screen = in front).
    /// </summary>
    public static class SortingLayerNames
    {
        /// <summary>Room floor and wall artwork.</summary>
        public const string Background = "Background";

        /// <summary>Unity's built-in layer; unassigned renderers end up here, above the room art.</summary>
        public const string Default = "Default";

        /// <summary>Stains, scorch marks, debris lying on the floor.</summary>
        public const string FloorDecals = "FloorDecals";

        /// <summary>Blob shadows under characters and objects.</summary>
        public const string Shadows = "Shadows";

        /// <summary>Characters, enemies, obstacles, pickups. Y-sorted.</summary>
        public const string Entities = "Entities";

        public const string Projectiles = "Projectiles";

        /// <summary>Hit flashes, explosions, muzzle effects.</summary>
        public const string Effects = "Effects";

        /// <summary>Things drawn over everything in the world (door frames, overhangs, wall tops).</summary>
        public const string Overhead = "Overhead";

        /// <summary>World-space UI such as enemy health bars and damage numbers. Screen UI uses canvases.</summary>
        public const string WorldUI = "WorldUI";
    }
}
