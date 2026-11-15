using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>Tuning for <see cref="CharacterAim2D"/>. Distances in world units.</summary>
    [CreateAssetMenu(fileName = "CharacterAim2DSettings", menuName = "Vyrden/Aiming/Character Aim 2D Settings")]
    public class CharacterAim2DSettings : ScriptableObject
    {
        [Tooltip("An aim point closer than this to the aim origin (pointer on top of the character) keeps the previous direction.")]
        [SerializeField, Min(0.01f)] private float _minimumAimDistance = 0.3f;

        public float MinimumAimDistance => _minimumAimDistance;
    }
}
