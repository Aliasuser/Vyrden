using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Reuses projectile instances per prefab, so firing does not instantiate and destroy objects every shot.
    /// Instances live under a "Projectiles" object in the active scene and disappear with it; destroyed
    /// entries are skipped.
    /// </summary>
    public static class ProjectilePool
    {
        private static readonly Dictionary<Projectile2D, Stack<Projectile2D>> Pools = new Dictionary<Projectile2D, Stack<Projectile2D>>();
        private static Transform _root;

        /// <summary>An inactive instance of <paramref name="prefab"/>; call <see cref="Projectile2D.Launch"/> to fire it.</summary>
        public static Projectile2D Get(Projectile2D prefab)
        {
            if (Pools.TryGetValue(prefab, out Stack<Projectile2D> pool))
            {
                while (pool.Count > 0)
                {
                    Projectile2D pooled = pool.Pop();
                    if (pooled != null)
                    {
                        return pooled;
                    }
                }
            }

            Projectile2D instance = Object.Instantiate(prefab, GetRoot());
            instance.gameObject.SetActive(false);
            instance.SourcePrefab = prefab;
            return instance;
        }

        /// <summary>Deactivates an instance and keeps it for the next shot.</summary>
        public static void Release(Projectile2D instance)
        {
            if (instance == null)
            {
                return;
            }

            if (instance.SourcePrefab == null)
            {
                Object.Destroy(instance.gameObject);
                return;
            }

            instance.gameObject.SetActive(false);
            if (!Pools.TryGetValue(instance.SourcePrefab, out Stack<Projectile2D> pool))
            {
                pool = new Stack<Projectile2D>();
                Pools.Add(instance.SourcePrefab, pool);
            }

            pool.Push(instance);
        }

        private static Transform GetRoot()
        {
            if (_root == null)
            {
                _root = new GameObject("Projectiles").transform;
            }

            return _root;
        }

        // Keeps the pool clean when domain reload is disabled for faster Play mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPools()
        {
            Pools.Clear();
            _root = null;
        }
    }
}
