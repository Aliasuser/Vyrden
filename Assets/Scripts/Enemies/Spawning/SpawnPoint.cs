using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>A fixed spawn spot with optional random scatter around it.</summary>
    public class SpawnPoint : SpawnLocation
    {
        [Tooltip("Random offset radius around the point, so enemies don't stack exactly.")]
        [SerializeField, Min(0f)] private float _scatterRadius = 1f;

        public override Vector3 SamplePoint(System.Random random)
        {
            float angle = Range(random, 0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt((float)random.NextDouble()) * _scatterRadius;
            return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.25f, _scatterRadius));
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }
    }
}
