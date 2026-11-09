using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Axis-aligned box that marks the playable arena. Spawners reject any position outside it.
    /// </summary>
    public class ArenaBounds : MonoBehaviour
    {
        [SerializeField] private Vector3 _size = new Vector3(56f, 20f, 56f);

        public Bounds WorldBounds => new Bounds(transform.position, _size);

        public bool Contains(Vector3 position)
        {
            return WorldBounds.Contains(position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.5f);
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}
