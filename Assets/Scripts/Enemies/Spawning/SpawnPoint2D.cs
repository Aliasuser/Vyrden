using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>A fixed 2D spawn spot with optional random scatter around it.</summary>
    public class SpawnPoint2D : SpawnLocation2D
    {
        [Tooltip("Random offset radius around the point, so enemies don't stack exactly.")]
        [SerializeField, Min(0f)] private float _scatterRadius = 0.5f;

        public float ScatterRadius => _scatterRadius;

        public override Vector2 SamplePoint(System.Random random)
        {
            float angle = Range(random, 0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt((float)random.NextDouble()) * _scatterRadius;
            return Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.15f, _scatterRadius));
        }
    }
}
