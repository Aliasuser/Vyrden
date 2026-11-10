using UnityEngine;

namespace Vyrden.Progression
{
    /// <summary>
    /// Experience needed per level. Levels covered by the table use its values directly; levels past the
    /// end grow from the last entry by <see cref="_growthPastTable"/> per level, so the table only has to
    /// cover the early game.
    /// </summary>
    [CreateAssetMenu(fileName = "ExperienceCurve", menuName = "Vyrden/Progression/Experience Curve")]
    public class ExperienceCurveDefinition : ScriptableObject, IExperienceCurve
    {
        private const float MinRequirement = 1f;

        [Tooltip("Element 0 = experience from level 1 to 2, element 1 = level 2 to 3, and so on.")]
        [SerializeField] private float[] _requirements = { 20f, 35f, 55f, 80f, 110f };
        [Tooltip("Each level past the table costs this much more than the one before (1.15 = +15%).")]
        [SerializeField, Min(1f)] private float _growthPastTable = 1.15f;
        [Tooltip("Highest reachable level. 0 = no cap.")]
        [SerializeField, Min(0)] private int _maxLevel;

        public int MaxLevel => _maxLevel;

        public float GetRequirement(int level)
        {
            if (_requirements == null || _requirements.Length == 0)
            {
                return MinRequirement;
            }

            int index = Mathf.Max(0, level - 1);
            if (index < _requirements.Length)
            {
                return Mathf.Max(MinRequirement, _requirements[index]);
            }

            float last = Mathf.Max(MinRequirement, _requirements[_requirements.Length - 1]);
            int levelsPastTable = index - (_requirements.Length - 1);
            return last * Mathf.Pow(_growthPastTable, levelsPastTable);
        }

        private void OnValidate()
        {
            _growthPastTable = Mathf.Max(1f, _growthPastTable);
            if (_requirements == null)
            {
                return;
            }

            for (int i = 0; i < _requirements.Length; i++)
            {
                _requirements[i] = Mathf.Max(MinRequirement, _requirements[i]);
            }
        }
    }
}
