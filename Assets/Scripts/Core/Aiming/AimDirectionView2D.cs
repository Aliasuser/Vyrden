using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// Presentation only: rotates this transform (around Z) to point along a <see cref="CharacterAim2D"/>'s
    /// aim direction. Used for the placeholder aim pointer; the final character sprite will pick a
    /// directional frame with <see cref="AimMath.ToSectorIndex"/> instead of rotating.
    /// </summary>
    public class AimDirectionView2D : MonoBehaviour
    {
        [Tooltip("Aim to follow. Defaults to the first CharacterAim2D in the parents.")]
        [SerializeField] private CharacterAim2D _aim;

        private void Awake()
        {
            if (_aim == null)
            {
                _aim = GetComponentInParent<CharacterAim2D>();
            }

            if (_aim == null)
            {
                Debug.LogError($"{nameof(AimDirectionView2D)} on '{name}' has no {nameof(CharacterAim2D)} to follow.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_aim == null)
            {
                return;
            }

            _aim.AimDirectionChanged += Apply;
            Apply(_aim.AimDirection);
        }

        private void OnDisable()
        {
            if (_aim != null)
            {
                _aim.AimDirectionChanged -= Apply;
            }
        }

        private void Apply(Vector2 direction)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, AimMath.ToAngle(direction));
        }
    }
}
