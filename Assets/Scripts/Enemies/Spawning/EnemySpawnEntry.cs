using System;
using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>One enemy type in a spawn table and how often it is picked relative to the others.</summary>
    [Serializable]
    public struct EnemySpawnEntry
    {
        [Tooltip("Any enemy prefab. A Health component lets the spawner free the slot on death.")]
        public GameObject Prefab;

        [Min(0f)]
        public float Weight;
    }
}
