using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>An axis-aligned rectangle on the XY plane, centred on this transform, where enemies may spawn.</summary>
    public class SpawnArea2D : SpawnLocation2D
    {
        [SerializeField] private Vector2 _size = new Vector2(3f, 2f);

        public Vector2 Size => _size;
        public Rect WorldRect => new Rect(Position - _size * 0.5f, _size);

        public override Vector2 SamplePoint(System.Random random)
        {
            return Position + new Vector2(Range(random, -0.5f, 0.5f) * _size.x, Range(random, -0.5f, 0.5f) * _size.y);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.6f);
            Gizmos.DrawWireCube(transform.position, new Vector3(_size.x, _size.y, 0f));
        }
    }
}
