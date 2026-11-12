using System;
using UnityEngine;

namespace Vyrden.Movement
{
    /// <summary>
    /// Top-down movement on the XY plane with a Dynamic <see cref="Rigidbody2D"/>. Turns a
    /// <see cref="MovementIntent"/> (from player input or AI) into velocity with acceleration and
    /// deceleration; walls and other bodies are handled by the 2D physics solver. Input length is capped at 1,
    /// so diagonals are never faster than straight movement. Knows nothing about input devices.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class CharacterMotor2D : MonoBehaviour, IMovementSpeedReceiver
    {
        [SerializeField] private CharacterMotor2DSettings _settings;

        private Rigidbody2D _body;
        private Vector2 _moveInput;
        private Vector2 _facing = Vector2.down;
        private float _speedMultiplier = 1f;

        /// <summary>Raised when the facing direction changes (for sprite direction later). Arg: new facing.</summary>
        public event Action<Vector2> FacingChanged;

        public CharacterMotor2DSettings Settings => _settings;
        public Rigidbody2D Body => _body;
        public Vector2 Velocity => _body != null ? _body.linearVelocity : Vector2.zero;
        public Vector2 MoveInput => _moveInput;
        public bool IsMoving => Velocity.sqrMagnitude > 0.0001f;
        public float SpeedMultiplier => _speedMultiplier;

        /// <summary>Last direction the character moved in, unit length. Starts facing down (towards the camera).</summary>
        public Vector2 Facing => _facing;

        /// <summary>Current top speed after multipliers.</summary>
        public float MaxSpeed => _settings != null ? _settings.MoveSpeed * _speedMultiplier : 0f;

        private void Reset()
        {
            ConfigureBody(GetComponent<Rigidbody2D>());
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            ConfigureBody(_body);
            if (_settings == null)
            {
                Debug.LogError($"{nameof(CharacterMotor2D)} on '{name}' has no {nameof(CharacterMotor2DSettings)} assigned.", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>Sets what the character wants to do. Only the X and Y of the move direction are used.</summary>
        public void SetIntent(in MovementIntent intent)
        {
            Vector2 move = new Vector2(intent.MoveDirection.x, intent.MoveDirection.y);
            _moveInput = move.sqrMagnitude > 1f ? move.normalized : move;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0f, multiplier);
        }

        private void FixedUpdate()
        {
            float deadZone = _settings.InputDeadZone;
            bool hasInput = _moveInput.sqrMagnitude > deadZone * deadZone;

            Vector2 target = hasInput ? _moveInput * MaxSpeed : Vector2.zero;
            float rate = hasInput ? _settings.Acceleration : _settings.Deceleration;

            // Start from the body's real velocity, so being stopped by a wall or pushed is respected.
            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity, target, rate * Time.fixedDeltaTime);

            if (hasInput)
            {
                Vector2 facing = _moveInput.normalized;
                if (facing != _facing)
                {
                    _facing = facing;
                    FacingChanged?.Invoke(_facing);
                }
            }
        }

        /// <summary>The body setup top-down characters need: no gravity, no spinning, smooth rendering.</summary>
        private static void ConfigureBody(Rigidbody2D body)
        {
            if (body == null)
            {
                return;
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.linearDamping = 0f;
        }
    }
}
