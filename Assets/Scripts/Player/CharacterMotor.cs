using System;
using UnityEngine;

namespace Vyrden.Player
{
    /// <summary>
    /// Moves a character with a <see cref="CharacterController"/> based on a <see cref="MovementIntent"/>.
    /// Knows nothing about input devices or cameras, so players and AI can drive it the same way.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : MonoBehaviour
    {
        private const float MinMoveSqr = 0.0001f;

        [SerializeField] private CharacterMotorSettings _settings;

        private CharacterController _controller;
        private MovementIntent _intent;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _timeSinceGrounded;
        private float _jumpBufferTimer;
        private bool _isGrounded;
        private Vector3 _groundNormal = Vector3.up;

        /// <summary>Raised when the character leaves the ground by jumping.</summary>
        public event Action Jumped;

        /// <summary>Raised when the character touches the ground after being airborne.</summary>
        public event Action Landed;

        public bool IsGrounded => _isGrounded;
        public bool IsSprinting => _intent.Sprint && _horizontalVelocity.sqrMagnitude > MinMoveSqr;
        public Vector3 Velocity => _horizontalVelocity + Vector3.up * _verticalVelocity;
        public CharacterMotorSettings Settings => _settings;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (_settings == null)
            {
                Debug.LogError($"{nameof(CharacterMotor)} on '{name}' has no {nameof(CharacterMotorSettings)} assigned.", this);
                enabled = false;
            }
        }

        /// <summary>Sets what the character should do. Call once per frame from an input source or AI.</summary>
        public void SetIntent(in MovementIntent intent)
        {
            _intent = intent;
            _intent.MoveDirection.y = 0f;
            _intent.MoveDirection = Vector3.ClampMagnitude(_intent.MoveDirection, 1f);

            if (intent.JumpRequested)
            {
                _jumpBufferTimer = _settings.JumpBufferTime;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            bool wasGrounded = _isGrounded;
            UpdateGroundState();

            UpdateHorizontalVelocity(deltaTime);
            UpdateVerticalVelocity(deltaTime);
            UpdateRotation(deltaTime);

            CollisionFlags flags = _controller.Move(GetMoveVelocity() * deltaTime);

            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
            {
                _verticalVelocity = 0f;
            }

            if (!wasGrounded && _isGrounded)
            {
                Landed?.Invoke();
            }
        }

        private void UpdateGroundState()
        {
            _groundNormal = Vector3.up;

            // Moving upward (jumping) never counts as grounded, otherwise the jump would be cancelled.
            if (_verticalVelocity > 0f)
            {
                _isGrounded = false;
            }
            else
            {
                float radius = _controller.radius * 0.95f;
                Vector3 origin = transform.TransformPoint(_controller.center);
                float castDistance = _controller.height * 0.5f - radius + _controller.skinWidth + _settings.GroundCheckDistance;

                bool hit = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit groundHit, castDistance,
                    _settings.GroundLayers, QueryTriggerInteraction.Ignore);

                if (hit && Vector3.Angle(groundHit.normal, Vector3.up) <= _controller.slopeLimit)
                {
                    _isGrounded = true;
                    _groundNormal = groundHit.normal;
                }
                else
                {
                    _isGrounded = _controller.isGrounded && !hit;
                }
            }

            _timeSinceGrounded = _isGrounded ? 0f : _timeSinceGrounded + Time.deltaTime;
        }

        private void UpdateHorizontalVelocity(float deltaTime)
        {
            bool hasInput = _intent.MoveDirection.sqrMagnitude > MinMoveSqr;
            float speed = _intent.Sprint ? _settings.SprintSpeed : _settings.WalkSpeed;
            Vector3 targetVelocity = _intent.MoveDirection * speed;

            float rate = hasInput ? _settings.Acceleration : _settings.Deceleration;
            if (!_isGrounded)
            {
                rate *= _settings.AirControl;
            }

            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, rate * deltaTime);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaTime);

            bool canJump = _isGrounded || _timeSinceGrounded <= _settings.CoyoteTime;
            if (_jumpBufferTimer > 0f && canJump && _verticalVelocity <= 0f)
            {
                _verticalVelocity = Mathf.Sqrt(2f * _settings.Gravity * _settings.JumpHeight);
                _jumpBufferTimer = 0f;
                _timeSinceGrounded = _settings.CoyoteTime + 1f; // Prevent a second jump from coyote time.
                _isGrounded = false;
                Jumped?.Invoke();
                return;
            }

            if (_isGrounded)
            {
                _verticalVelocity = -_settings.GroundStickSpeed;
                return;
            }

            _verticalVelocity = Mathf.Max(_verticalVelocity - _settings.Gravity * deltaTime, -_settings.MaxFallSpeed);
        }

        private void UpdateRotation(float deltaTime)
        {
            if (_intent.MoveDirection.sqrMagnitude <= MinMoveSqr)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(_intent.MoveDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _settings.TurnSpeed * deltaTime);
        }

        private Vector3 GetMoveVelocity()
        {
            Vector3 horizontal = _horizontalVelocity;

            // Follow the slope surface so walking downhill doesn't bounce and uphill keeps its speed.
            if (_isGrounded && horizontal.sqrMagnitude > MinMoveSqr)
            {
                horizontal = Vector3.ProjectOnPlane(horizontal, _groundNormal).normalized * horizontal.magnitude;
            }

            return horizontal + Vector3.up * _verticalVelocity;
        }
    }
}
