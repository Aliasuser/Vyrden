using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>Tuning for <see cref="OverheadCamera2D"/>. One asset is shared by every 2D gameplay scene.</summary>
    [CreateAssetMenu(fileName = "OverheadCameraSettings", menuName = "Vyrden/Camera/Overhead Camera Settings")]
    public class OverheadCameraSettings : ScriptableObject
    {
        [Header("View")]
        [Tooltip("World area (units) that must always be visible. The orthographic size is chosen so this fits at any aspect ratio. Default: a 15 × 9 room plus a small margin.")]
        [SerializeField] private Vector2 _minimumVisibleArea = new Vector2(15.5f, 9.5f);

        [Header("Following")]
        [Tooltip("Approximate seconds to catch up with the target. 0 = no smoothing.")]
        [SerializeField, Min(0f)] private float _followSmoothTime = 0.15f;
        [Tooltip("Top follow speed in units per second.")]
        [SerializeField, Min(0.1f)] private float _maxFollowSpeed = 60f;

        [Header("Room transitions")]
        [Tooltip("Seconds a pan to another room takes. 0 = cut instantly.")]
        [SerializeField, Min(0f)] private float _transitionDuration = 0.35f;
        [Tooltip("Easing over the transition (0..1 time → 0..1 progress).")]
        [SerializeField] private AnimationCurve _transitionEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public Vector2 MinimumVisibleArea => _minimumVisibleArea;
        public float FollowSmoothTime => _followSmoothTime;
        public float MaxFollowSpeed => _maxFollowSpeed;
        public float TransitionDuration => _transitionDuration;
        public AnimationCurve TransitionEasing => _transitionEasing;

        private void OnValidate()
        {
            _minimumVisibleArea = Vector2.Max(_minimumVisibleArea, new Vector2(1f, 1f));
        }
    }
}
