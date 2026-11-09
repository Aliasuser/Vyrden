using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>A rectangular spawn region (local X/Z), rotated and positioned with its transform.</summary>
    public class SpawnArea : SpawnLocation
    {
        [SerializeField] private Vector2 _size = new Vector2(10f, 10f);

        public override Vector3 SamplePoint(System.Random random)
        {
            var local = new Vector3(Range(random, -0.5f, 0.5f) * _size.x, 0f, Range(random, -0.5f, 0.5f) * _size.y);
            return transform.TransformPoint(local);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.6f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_size.x, 0.1f, _size.y));
        }
    }
}
