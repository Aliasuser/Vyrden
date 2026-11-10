using UnityEngine;

namespace Vyrden.World
{
    /// <summary>Pure charging rules for the teleporter event. Progress is a fraction from 0 to 1.</summary>
    public static class TeleporterChargeCalculator
    {
        /// <summary>
        /// Advances progress by <paramref name="deltaTime"/>: charges while a player is in the zone (slower
        /// when contested), decays while nobody is. Never leaves 0..1.
        /// </summary>
        public static float Step(TeleporterEncounterSettings settings, float progress, float deltaTime, bool playerInZone, int enemiesInZone)
        {
            if (settings == null || deltaTime <= 0f)
            {
                return Mathf.Clamp01(progress);
            }

            if (playerInZone)
            {
                float rate = 1f / settings.ChargeDuration;
                if (IsContested(settings, enemiesInZone))
                {
                    rate *= settings.ContestedChargeMultiplier;
                }

                progress += rate * deltaTime;
            }
            else
            {
                progress -= settings.DecayPerSecond * deltaTime;
            }

            return Mathf.Clamp01(progress);
        }

        public static bool IsContested(TeleporterEncounterSettings settings, int enemiesInZone)
        {
            return settings.ContestedEnemyCount > 0 && enemiesInZone >= settings.ContestedEnemyCount;
        }

        /// <summary>Progress after an encounter enemy dies.</summary>
        public static float AddKill(TeleporterEncounterSettings settings, float progress)
        {
            return settings == null ? progress : Mathf.Clamp01(progress + settings.ProgressPerKill);
        }
    }
}
