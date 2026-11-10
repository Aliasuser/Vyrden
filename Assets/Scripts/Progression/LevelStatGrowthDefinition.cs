using System.Collections.Generic;
using UnityEngine;
using Vyrden.Stats;

namespace Vyrden.Progression
{
    /// <summary>Which stats a character gains per level. Level 1 gives nothing; each level above adds one step.</summary>
    [CreateAssetMenu(fileName = "LevelStatGrowth", menuName = "Vyrden/Progression/Level Stat Growth")]
    public class LevelStatGrowthDefinition : ScriptableObject
    {
        [SerializeField] private List<LevelStatGrowth> _growth = new List<LevelStatGrowth>();

        public IReadOnlyList<LevelStatGrowth> Growth => _growth;

        /// <summary>Adds the modifiers for <paramref name="level"/> to <paramref name="output"/>.</summary>
        public void CollectStatModifiers(int level, List<StatModifier> output, object source)
        {
            int levelsAboveFirst = level - 1;
            if (levelsAboveFirst <= 0)
            {
                return;
            }

            for (int i = 0; i < _growth.Count; i++)
            {
                LevelStatGrowth growth = _growth[i];
                if (!Mathf.Approximately(growth.ValuePerLevel, 0f))
                {
                    output.Add(new StatModifier(growth.Stat, growth.ModifierType, growth.ValuePerLevel * levelsAboveFirst, source));
                }
            }
        }
    }
}
