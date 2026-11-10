using System;
using UnityEngine;

namespace Vyrden.Progression
{
    /// <summary>A named difficulty band, reached when the coefficient passes <see cref="MinCoefficient"/>.</summary>
    [Serializable]
    public struct DifficultyTier
    {
        [SerializeField] private string _name;
        [SerializeField, Min(0f)] private float _minCoefficient;

        public DifficultyTier(string name, float minCoefficient)
        {
            _name = name;
            _minCoefficient = minCoefficient;
        }

        public string Name => _name;
        public float MinCoefficient => _minCoefficient;
    }
}
