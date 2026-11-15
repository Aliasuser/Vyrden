using System;
using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// Where a character is aiming on the XY plane, kept separate from movement: moving never changes
    /// the aim and aiming never changes the movement direction. Fed a world aim point (mouse) or a
    /// direction (gamepad stick, AI); weapons read <see cref="AimDirection"/> and <see cref="Origin"/>,
    /// presentation listens to <see cref="AimDirectionChanged"/>. Knows nothing about input devices.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterAim2D : MonoBehaviour
    {
        [SerializeField] private CharacterAim2DSettings _settings;
        [Tooltip("Point the aim is measured from (e.g. the body centre). Defaults to this transform.")]
        [SerializeField] private Transform _aimOrigin;

        private Vector2 _aimDirection = Vector2.down;
        private Vector2 _aimPoint;

        /// <summary>Raised when the aim direction changes. Arg: new unit direction.</summary>
        public event Action<Vector2> AimDirectionChanged;

        /// <summary>Current aim direction, unit length. Starts down (towards the camera), like movement facing.</summary>
        public Vector2 AimDirection => _aimDirection;

        /// <summary>Angle of <see cref="AimDirection"/> in degrees, 0 = right, counter-clockwise.</summary>
        public float AimAngle => AimMath.ToAngle(_aimDirection);

        /// <summary>Last world point aimed at (from <see cref="SetAimPoint"/>, or projected from a direction).</summary>
        public Vector2 AimPoint => _aimPoint;

        /// <summary>World position aiming is measured from; weapons spawn shots from here or an offset of it.</summary>
        public Vector2 Origin => WorldPlane.ToPlanar(_aimOrigin.position);

        public CharacterAim2DSettings Settings => _settings;

        private void Awake()
        {
            if (_aimOrigin == null)
            {
                _aimOrigin = transform;
            }

            if (_settings == null)
            {
                Debug.LogError($"{nameof(CharacterAim2D)} on '{name}' has no {nameof(CharacterAim2DSettings)} assigned.", this);
                enabled = false;
                return;
            }

            _aimPoint = Origin + _aimDirection;
        }

        /// <summary>
        /// Aim at a world point. Call every frame while the point is driven (the origin moves with the
        /// character). A point on top of the character keeps the previous direction.
        /// </summary>
        public void SetAimPoint(Vector2 worldPoint)
        {
            if (!enabled)
            {
                return;
            }

            _aimPoint = worldPoint;
            ApplyDirection(AimMath.DirectionTo(Origin, worldPoint, _settings.MinimumAimDistance, _aimDirection));
        }

        /// <summary>Aim in a direction (stick, AI). Zero keeps the previous direction.</summary>
        public void SetAimDirection(Vector2 direction)
        {
            if (!enabled || direction.sqrMagnitude < 1e-8f)
            {
                return;
            }

            ApplyDirection(direction.normalized);
            _aimPoint = Origin + _aimDirection;
        }

        private void ApplyDirection(Vector2 direction)
        {
            if (direction == _aimDirection)
            {
                return;
            }

            _aimDirection = direction;
            AimDirectionChanged?.Invoke(_aimDirection);
        }
    }
}
