using UnityEngine;
using UnityEngine.UI;
using Vyrden.Combat;

namespace Vyrden.UI
{
    /// <summary>
    /// Flashes a crosshair marker when a weapon deals damage. Only observes the weapon.
    /// </summary>
    public class HitMarkerUI : MonoBehaviour
    {
        [SerializeField] private Weapon _weapon;
        [SerializeField] private Graphic _marker;
        [SerializeField, Min(0.01f)] private float _showDuration = 0.12f;
        [SerializeField] private Color _hitColor = Color.white;
        [SerializeField] private Color _killColor = new Color(1f, 0.2f, 0.2f);

        private float _timeLeft;
        private Color _currentColor;

        private void OnEnable()
        {
            if (_weapon == null || _marker == null)
            {
                Debug.LogWarning($"{nameof(HitMarkerUI)} on '{name}' needs a weapon and a marker graphic.", this);
                enabled = false;
                return;
            }

            _weapon.Fired += OnFired;
            SetAlpha(0f);
        }

        private void OnDisable()
        {
            if (_weapon != null)
            {
                _weapon.Fired -= OnFired;
            }
        }

        private void OnFired(ShotResult shot)
        {
            if (shot.DamageDealt <= 0f)
            {
                return;
            }

            _currentColor = shot.Killed ? _killColor : _hitColor;
            _timeLeft = _showDuration;
            SetAlpha(1f);
        }

        private void Update()
        {
            if (_timeLeft <= 0f)
            {
                return;
            }

            _timeLeft -= Time.deltaTime;
            SetAlpha(Mathf.Clamp01(_timeLeft / _showDuration));
        }

        private void SetAlpha(float alpha)
        {
            Color color = _currentColor;
            color.a = alpha;
            _marker.color = color;
        }
    }
}
