using UnityEngine;

namespace Vyrden.Player
{
    /// <summary>
    /// Orbit camera that follows a target. Receives look input from outside and never rotates the target.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        private const int MaxCollisionHits = 8;

        [SerializeField] private Transform _target;

        [Header("Sensitivity")]
        [Tooltip("Degrees per pixel of mouse movement.")]
        [SerializeField, Min(0f)] private float _mouseSensitivity = 0.12f;
        [Tooltip("Degrees per second at full gamepad stick deflection.")]
        [SerializeField, Min(0f)] private float _stickSensitivity = 180f;
        [SerializeField] private bool _invertY;

        [Header("Orbit")]
        [SerializeField, Min(0.5f)] private float _followDistance = 5f;
        [SerializeField, Range(-89f, 0f)] private float _minPitch = -35f;
        [SerializeField, Range(0f, 89f)] private float _maxPitch = 70f;
        [SerializeField, Range(-89f, 89f)] private float _startPitch = 15f;

        [Header("Follow")]
        [Tooltip("Seconds to catch up with the target. 0 = locked to target.")]
        [SerializeField, Min(0f)] private float _followSmoothTime = 0.05f;

        [Header("Collision")]
        [SerializeField, Min(0f)] private float _collisionRadius = 0.25f;
        [SerializeField] private LayerMask _collisionLayers = ~0;
        [Tooltip("How fast the camera moves back out after an obstacle clears (units per second).")]
        [SerializeField, Min(0f)] private float _distanceRecoverySpeed = 8f;

        private readonly RaycastHit[] _hits = new RaycastHit[MaxCollisionHits];
        private float _yaw;
        private float _pitch;
        private Vector3 _focusPoint;
        private Vector3 _focusVelocity;
        private float _currentDistance;

        public Transform Target => _target;
        public float Yaw => _yaw;
        public float Pitch => _pitch;

        /// <summary>Camera forward flattened onto the ground plane.</summary>
        public Vector3 PlanarForward => Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;

        /// <summary>Camera right flattened onto the ground plane.</summary>
        public Vector3 PlanarRight => Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;

        private void Start()
        {
            SetTarget(_target);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            _pitch = Mathf.Clamp(_startPitch, _minPitch, _maxPitch);
            _currentDistance = _followDistance;

            if (_target != null)
            {
                _yaw = _target.eulerAngles.y;
                _focusPoint = _target.position;
                _focusVelocity = Vector3.zero;
                ApplyTransform();
            }
        }

        /// <summary>Rotate by a pointer delta in pixels (mouse). Already frame-independent.</summary>
        public void AddLookDelta(Vector2 pointerDelta)
        {
            Rotate(pointerDelta * _mouseSensitivity);
        }

        /// <summary>Rotate by a stick value in -1..1. Scaled by delta time.</summary>
        public void AddLookRate(Vector2 stickValue)
        {
            Rotate(stickValue * (_stickSensitivity * Time.deltaTime));
        }

        private void Rotate(Vector2 degrees)
        {
            _yaw = Mathf.Repeat(_yaw + degrees.x, 360f);
            float pitchDelta = _invertY ? degrees.y : -degrees.y;
            _pitch = Mathf.Clamp(_pitch + pitchDelta, _minPitch, _maxPitch);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            _focusPoint = _followSmoothTime > 0f
                ? Vector3.SmoothDamp(_focusPoint, _target.position, ref _focusVelocity, _followSmoothTime)
                : _target.position;

            ApplyTransform();
        }

        private void ApplyTransform()
        {
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 back = rotation * Vector3.back;

            float allowedDistance = GetUnobstructedDistance(back);
            _currentDistance = allowedDistance < _currentDistance
                ? allowedDistance
                : Mathf.MoveTowards(_currentDistance, allowedDistance, _distanceRecoverySpeed * Time.deltaTime);

            transform.SetPositionAndRotation(_focusPoint + back * _currentDistance, rotation);
        }

        private float GetUnobstructedDistance(Vector3 direction)
        {
            int count = Physics.SphereCastNonAlloc(_focusPoint, _collisionRadius, direction, _hits, _followDistance,
                _collisionLayers, QueryTriggerInteraction.Ignore);

            float closest = _followDistance;
            Transform targetRoot = _target.root;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];
                if (hit.transform.IsChildOf(targetRoot))
                {
                    continue; // Ignore the character we are following.
                }

                if (hit.distance > 0f && hit.distance < closest)
                {
                    closest = hit.distance;
                }
            }

            return closest;
        }
    }
}
