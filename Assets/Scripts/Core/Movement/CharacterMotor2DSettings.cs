using UnityEngine;

namespace Vyrden.Movement
{
    /// <summary>Tuning for <see cref="CharacterMotor2D"/>. Speeds in units per second.</summary>
    [CreateAssetMenu(fileName = "CharacterMotor2DSettings", menuName = "Vyrden/Movement/Character Motor 2D Settings")]
    public class CharacterMotor2DSettings : ScriptableObject
    {
        [Tooltip("Top speed at full input, before speed multipliers.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 5f;
        [Tooltip("How fast velocity moves towards the input direction while there is input (units/s²).")]
        [SerializeField, Min(0.01f)] private float _acceleration = 60f;
        [Tooltip("How fast velocity drops to zero with no input (units/s²).")]
        [SerializeField, Min(0.01f)] private float _deceleration = 70f;
        [Tooltip("Input below this length counts as no input (stick drift).")]
        [SerializeField, Range(0f, 0.5f)] private float _inputDeadZone = 0.1f;

        public float MoveSpeed => _moveSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float InputDeadZone => _inputDeadZone;
    }
}
