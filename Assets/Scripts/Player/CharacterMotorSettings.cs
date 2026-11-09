using UnityEngine;

namespace Vyrden.Player
{
    /// <summary>
    /// Tuning data for <see cref="CharacterMotor"/>. Shared asset so several characters can use one profile.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterMotorSettings", menuName = "Vyrden/Character Motor Settings")]
    public class CharacterMotorSettings : ScriptableObject
    {
        [Header("Speed")]
        [SerializeField, Min(0f)] private float _walkSpeed = 6f;
        [SerializeField, Min(0f)] private float _sprintSpeed = 10f;
        [Tooltip("Units per second squared when speeding up.")]
        [SerializeField, Min(0f)] private float _acceleration = 50f;
        [Tooltip("Units per second squared when slowing down.")]
        [SerializeField, Min(0f)] private float _deceleration = 60f;
        [Tooltip("Multiplier applied to acceleration while airborne.")]
        [SerializeField, Range(0f, 1f)] private float _airControl = 0.4f;

        [Header("Rotation")]
        [Tooltip("Degrees per second the character turns toward its movement direction.")]
        [SerializeField, Min(0f)] private float _turnSpeed = 720f;

        [Header("Jump and Gravity")]
        [SerializeField, Min(0f)] private float _jumpHeight = 1.6f;
        [Tooltip("Downward acceleration in units per second squared (positive value).")]
        [SerializeField, Min(0f)] private float _gravity = 30f;
        [SerializeField, Min(0f)] private float _maxFallSpeed = 50f;
        [Tooltip("Seconds after leaving the ground during which a jump is still allowed.")]
        [SerializeField, Min(0f)] private float _coyoteTime = 0.12f;
        [Tooltip("Seconds a jump press is remembered before landing.")]
        [SerializeField, Min(0f)] private float _jumpBufferTime = 0.12f;

        [Header("Ground Detection")]
        [Tooltip("Extra distance below the capsule that still counts as grounded.")]
        [SerializeField, Min(0f)] private float _groundCheckDistance = 0.15f;
        [Tooltip("Small downward speed applied while grounded to keep the character on slopes and steps.")]
        [SerializeField, Min(0f)] private float _groundStickSpeed = 2f;
        [SerializeField] private LayerMask _groundLayers = ~0;

        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float AirControl => _airControl;
        public float TurnSpeed => _turnSpeed;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float MaxFallSpeed => _maxFallSpeed;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;
        public float GroundCheckDistance => _groundCheckDistance;
        public float GroundStickSpeed => _groundStickSpeed;
        public LayerMask GroundLayers => _groundLayers;
    }
}
