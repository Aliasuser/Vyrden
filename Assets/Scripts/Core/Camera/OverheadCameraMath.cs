using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>Pure maths for the overhead camera: view size from an area to fit, and keeping the view inside bounds.</summary>
    public static class OverheadCameraMath
    {
        /// <summary>
        /// Orthographic size (half the view height) that shows at least <paramref name="visibleArea"/> at the
        /// given aspect ratio (width / height). Wide screens are limited by height, narrow ones by width.
        /// </summary>
        public static float CalculateOrthographicSize(Vector2 visibleArea, float aspect)
        {
            aspect = Mathf.Max(0.01f, aspect);
            return Mathf.Max(visibleArea.y * 0.5f, visibleArea.x * 0.5f / aspect);
        }

        /// <summary>Half the width and height of the view in world units.</summary>
        public static Vector2 HalfExtents(float orthographicSize, float aspect)
        {
            return new Vector2(orthographicSize * aspect, orthographicSize);
        }

        /// <summary>
        /// Moves a camera centre so the view stays inside <paramref name="bounds"/>. On an axis where the view is
        /// larger than the bounds, the camera is centred on the bounds instead, so the whole area stays visible.
        /// </summary>
        public static Vector2 ClampToBounds(Vector2 center, Rect bounds, Vector2 halfExtents)
        {
            return new Vector2(ClampAxis(center.x, bounds.xMin, bounds.xMax, halfExtents.x),
                ClampAxis(center.y, bounds.yMin, bounds.yMax, halfExtents.y));
        }

        private static float ClampAxis(float value, float min, float max, float halfExtent)
        {
            if (max - min <= halfExtent * 2f)
            {
                return (min + max) * 0.5f;
            }

            return Mathf.Clamp(value, min + halfExtent, max - halfExtent);
        }
    }
}
