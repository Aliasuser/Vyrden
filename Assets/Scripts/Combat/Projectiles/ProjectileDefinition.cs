using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// How a projectile flies: which prefab, how fast and for how long. Damage and fire rate belong to the
    /// weapon (<see cref="ProjectileWeaponDefinition"/>), so one projectile type can be shared by several weapons.
    /// </summary>
    [CreateAssetMenu(fileName = "Projectile", menuName = "Vyrden/Weapons/Projectile")]
    public class ProjectileDefinition : ScriptableObject
    {
        [Tooltip("Prefab with a Projectile2D. Instances are pooled.")]
        [SerializeField] private Projectile2D _prefab;
        [Tooltip("Travel speed in units per second.")]
        [SerializeField, Min(0.1f)] private float _speed = 10f;
        [Tooltip("Seconds before the projectile disappears if it hits nothing (range = speed × lifetime).")]
        [SerializeField, Min(0.01f)] private float _lifetime = 0.8f;

        public Projectile2D Prefab => _prefab;
        public float Speed => _speed;
        public float Lifetime => _lifetime;
        public float Range => _speed * _lifetime;

        private void OnValidate()
        {
            if (_prefab == null)
            {
                Debug.LogWarning($"{nameof(ProjectileDefinition)} '{name}' has no projectile prefab.", this);
            }
        }
    }
}
