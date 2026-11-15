using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>Weighted random choice from a list of <see cref="EnemySpawnEntry"/>, shared by the 3D and 2D spawn settings.</summary>
    public static class EnemySpawnTable
    {
        /// <summary>Returns null when the list is empty or every usable weight is zero.</summary>
        public static GameObject Pick(IReadOnlyList<EnemySpawnEntry> entries, System.Random random)
        {
            float total = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Prefab != null && entries[i].Weight > 0f)
                {
                    total += entries[i].Weight;
                }
            }

            if (total <= 0f)
            {
                return null;
            }

            float roll = (float)random.NextDouble() * total;
            for (int i = 0; i < entries.Count; i++)
            {
                EnemySpawnEntry entry = entries[i];
                if (entry.Prefab == null || entry.Weight <= 0f)
                {
                    continue;
                }

                roll -= entry.Weight;
                if (roll < 0f)
                {
                    return entry.Prefab;
                }
            }

            return null;
        }
    }
}
