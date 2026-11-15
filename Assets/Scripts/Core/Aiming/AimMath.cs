using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// Pure aiming math for the XY gameplay plane: screen pointer → plane point, point → direction, and
    /// direction → angle / sector (for directional sprites). Angles are degrees, 0 = right (+X),
    /// counter-clockwise, so 90 = up (+Y).
    /// </summary>
    public static class AimMath
    {
        /// <summary>
        /// Keeps a screen position inside a camera's pixel rect, so a pointer outside the game view still
        /// aims at the nearest visible edge instead of at a point the player cannot see.
        /// </summary>
        public static Vector2 ClampToRect(Vector2 screenPosition, Rect pixelRect)
        {
            return new Vector2(
                Mathf.Clamp(screenPosition.x, pixelRect.xMin, pixelRect.xMax),
                Mathf.Clamp(screenPosition.y, pixelRect.yMin, pixelRect.yMax));
        }

        /// <summary>
        /// Where a ray crosses the plane z = <paramref name="planeZ"/>. Works for orthographic and
        /// perspective cameras. False if the ray is parallel to the plane or points away from it.
        /// </summary>
        public static bool TryIntersectPlane(Ray ray, float planeZ, out Vector2 point)
        {
            float dz = ray.direction.z;
            if (Mathf.Abs(dz) < 1e-6f)
            {
                point = default;
                return false;
            }

            float distance = (planeZ - ray.origin.z) / dz;
            if (distance < 0f)
            {
                point = default;
                return false;
            }

            Vector3 hit = ray.origin + ray.direction * distance;
            point = new Vector2(hit.x, hit.y);
            return true;
        }

        /// <summary>
        /// Unit direction from <paramref name="origin"/> to <paramref name="target"/>. When the target is
        /// closer than <paramref name="minimumDistance"/> (pointer on top of the character) the direction
        /// is unreliable, so <paramref name="fallback"/> is returned instead.
        /// </summary>
        public static Vector2 DirectionTo(Vector2 origin, Vector2 target, float minimumDistance, Vector2 fallback)
        {
            Vector2 offset = target - origin;
            float minimum = Mathf.Max(minimumDistance, 1e-4f);
            return offset.sqrMagnitude < minimum * minimum ? fallback : offset.normalized;
        }

        /// <summary>Angle of a direction in degrees, 0..360, 0 = right, counter-clockwise.</summary>
        public static float ToAngle(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return angle < 0f ? angle + 360f : angle;
        }

        /// <summary>Unit direction for an angle in degrees (0 = right, counter-clockwise).</summary>
        public static Vector2 FromAngle(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        /// <summary>
        /// Which of <paramref name="sectorCount"/> equal sectors a direction falls in, for picking a
        /// directional sprite or animation. Sector 0 is centred on right, then counter-clockwise
        /// (with 4 sectors: 0 right, 1 up, 2 left, 3 down; with 8, odd indices are the diagonals).
        /// </summary>
        public static int ToSectorIndex(Vector2 direction, int sectorCount)
        {
            if (sectorCount <= 1)
            {
                return 0;
            }

            float sectorSize = 360f / sectorCount;
            int index = Mathf.FloorToInt((ToAngle(direction) + sectorSize * 0.5f) / sectorSize);
            return index % sectorCount;
        }
    }
}
