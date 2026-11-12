using System;
using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// Fixed straight-down orthographic camera for the XY gameplay plane. Never rotates or tilts; the view
    /// size comes from <see cref="OverheadCameraSettings.MinimumVisibleArea"/> so the scale is the same in
    /// every room and at any aspect ratio. Follows a target transform smoothly, optionally kept inside room
    /// bounds (a room that fits the view is simply centred), and pans between rooms with
    /// <see cref="TransitionTo"/>. It only reads the target's position, so it is independent of how the
    /// target moves.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class OverheadCamera2D : MonoBehaviour
    {
        [SerializeField] private OverheadCameraSettings _settings;
        [Tooltip("What to follow. Can be set at runtime with SetTarget.")]
        [SerializeField] private Transform _target;
        [Tooltip("Start with the view kept inside these bounds (e.g. the first room).")]
        [SerializeField] private bool _useInitialBounds;
        [SerializeField] private Rect _initialBounds = new Rect(-7.5f, -4.5f, 15f, 9f);

        private Camera _camera;
        private Vector2 _position;
        private Vector2 _velocity;
        private Rect _bounds;
        private bool _hasBounds;
        private float _aspect = -1f;

        private bool _isTransitioning;
        private Vector2 _transitionFrom;
        private Vector2 _transitionTo;
        private float _transitionTime;

        /// <summary>Raised when a pan to new bounds begins. Arg: the new bounds.</summary>
        public event Action<Rect> TransitionStarted;

        /// <summary>Raised when that pan ends.</summary>
        public event Action TransitionFinished;

        public Camera Camera => _camera;
        public Transform Target => _target;
        public bool HasBounds => _hasBounds;
        public Rect Bounds => _bounds;
        public bool IsTransitioning => _isTransitioning;
        public Vector2 Position => _position;

        /// <summary>Half the visible width and height in world units.</summary>
        public Vector2 ViewHalfExtents => OverheadCameraMath.HalfExtents(_camera.orthographicSize, _camera.aspect);

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_settings == null)
            {
                Debug.LogError($"{nameof(OverheadCamera2D)} on '{name}' has no {nameof(OverheadCameraSettings)} assigned.", this);
                enabled = false;
                return;
            }

            _camera.orthographic = true;
            _position = WorldPlane.ToPlanar(transform.position);
            if (_useInitialBounds)
            {
                SetBounds(_initialBounds);
            }

            UpdateViewSize();
            SnapToTarget();
        }

        private void LateUpdate()
        {
            // LateUpdate so the target (moved in Update / interpolated physics) has its final position this frame.
            UpdateViewSize();

            if (_isTransitioning)
            {
                StepTransition(Time.deltaTime);
            }
            else if (_target != null)
            {
                Vector2 desired = Clamp(WorldPlane.ToPlanar(_target.position));
                _position = _settings.FollowSmoothTime > 0f
                    ? Vector2.SmoothDamp(_position, desired, ref _velocity, _settings.FollowSmoothTime, _settings.MaxFollowSpeed, Time.deltaTime)
                    : desired;
            }

            ApplyTransform();
        }

        public void SetTarget(Transform target, bool snap = false)
        {
            _target = target;
            if (snap)
            {
                SnapToTarget();
            }
        }

        /// <summary>Keeps the view inside <paramref name="bounds"/> from now on (no pan; use <see cref="TransitionTo"/> to pan).</summary>
        public void SetBounds(Rect bounds)
        {
            _bounds = bounds;
            _hasBounds = true;
        }

        public void ClearBounds()
        {
            _hasBounds = false;
        }

        /// <summary>Jumps straight to the target (or stays put without one), ending any pan.</summary>
        public void SnapToTarget()
        {
            _isTransitioning = false;
            _velocity = Vector2.zero;
            _position = Clamp(_target != null ? WorldPlane.ToPlanar(_target.position) : _position);
            ApplyTransform();
        }

        /// <summary>
        /// Pans to new bounds (a room transition): eases from the current view to where the target will be
        /// framed inside <paramref name="bounds"/>, then resumes following there.
        /// </summary>
        public void TransitionTo(Rect bounds)
        {
            SetBounds(bounds);
            Vector2 focus = _target != null ? WorldPlane.ToPlanar(_target.position) : bounds.center;
            _transitionFrom = _position;
            _transitionTo = Clamp(focus);
            _transitionTime = 0f;
            _velocity = Vector2.zero;
            _isTransitioning = true;
            TransitionStarted?.Invoke(bounds);

            if (_settings.TransitionDuration <= 0f)
            {
                StepTransition(0f);
                ApplyTransform();
            }
        }

        private void StepTransition(float deltaTime)
        {
            _transitionTime += deltaTime;
            float duration = _settings.TransitionDuration;
            float t = duration > 0f ? Mathf.Clamp01(_transitionTime / duration) : 1f;
            _position = Vector2.LerpUnclamped(_transitionFrom, _transitionTo, _settings.TransitionEasing.Evaluate(t));

            if (t >= 1f)
            {
                _position = _transitionTo;
                _isTransitioning = false;
                TransitionFinished?.Invoke();
            }
        }

        private void UpdateViewSize()
        {
            float aspect = _camera.aspect;
            if (Mathf.Approximately(aspect, _aspect))
            {
                return;
            }

            _aspect = aspect;
            _camera.orthographicSize = OverheadCameraMath.CalculateOrthographicSize(_settings.MinimumVisibleArea, aspect);
        }

        private Vector2 Clamp(Vector2 center)
        {
            return _hasBounds ? OverheadCameraMath.ClampToBounds(center, _bounds, ViewHalfExtents) : center;
        }

        private void ApplyTransform()
        {
            transform.SetPositionAndRotation(new Vector3(_position.x, _position.y, WorldPlane.CameraZ), Quaternion.identity);
        }

        private void OnDrawGizmosSelected()
        {
            if (_useInitialBounds || _hasBounds)
            {
                Rect r = _hasBounds ? _bounds : _initialBounds;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0f), new Vector3(r.width, r.height, 0f));
            }
        }
    }
}
