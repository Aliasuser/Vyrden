using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// The 2D gameplay convention: everything plays on the XY plane at Z = 0. +X is right (east), +Y is
    /// up on screen (north), 1 unit = roughly one character's height = <see cref="PixelsPerUnit"/> pixels of art. Z is only used
    /// by the camera, which sits at negative Z looking towards +Z.
    /// </summary>
    public static class WorldPlane
    {
        public const float GameplayZ = 0f;

        /// <summary>Pixels per unit for all gameplay sprites (characters, enemies, rooms, effects).</summary>
        public const float PixelsPerUnit = 256f;

        /// <summary>Default Z of the gameplay camera.</summary>
        public const float CameraZ = -10f;

        /// <summary>World position on the gameplay plane.</summary>
        public static Vector3 ToWorld(Vector2 planar)
        {
            return new Vector3(planar.x, planar.y, GameplayZ);
        }

        /// <summary>Drops Z.</summary>
        public static Vector2 ToPlanar(Vector3 world)
        {
            return new Vector2(world.x, world.y);
        }

        /// <summary>Same point moved onto the gameplay plane.</summary>
        public static Vector3 Flatten(Vector3 world)
        {
            return new Vector3(world.x, world.y, GameplayZ);
        }
    }
}
