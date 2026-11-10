using UnityEngine;

namespace Vyrden.World
{
    /// <summary>
    /// Placeholder presentation: tints the teleporter by state and draws the zone as a ring while charging.
    /// Only listens to <see cref="TeleporterEncounter"/> events.
    /// </summary>
    [RequireComponent(typeof(TeleporterEncounter))]
    public class TeleporterVisuals : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] _tintedRenderers;
        [SerializeField] private LineRenderer _zoneRing;
        [SerializeField, Min(8)] private int _ringSegments = 64;
        [SerializeField] private Color _inactiveColor = new Color(0.35f, 0.35f, 0.4f);
        [SerializeField] private Color _chargingColor = new Color(1f, 0.55f, 0.1f);
        [SerializeField] private Color _chargedColor = new Color(0.2f, 0.9f, 1f);
        [SerializeField] private Color _ringOccupiedColor = new Color(1f, 0.55f, 0.1f);
        [SerializeField] private Color _ringEmptyColor = new Color(1f, 0.15f, 0.1f);

        private TeleporterEncounter _encounter;
        private MaterialPropertyBlock _block;

        private void Awake()
        {
            _encounter = GetComponent<TeleporterEncounter>();
            _block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _encounter.StateChanged += OnStateChanged;
            _encounter.PlayerInZoneChanged += OnPlayerInZoneChanged;
            BuildRing();
            OnStateChanged(_encounter.State);
        }

        private void OnDisable()
        {
            _encounter.StateChanged -= OnStateChanged;
            _encounter.PlayerInZoneChanged -= OnPlayerInZoneChanged;
        }

        private void OnStateChanged(TeleporterState state)
        {
            Color color = state == TeleporterState.Charging ? _chargingColor
                : state == TeleporterState.Charged ? _chargedColor
                : _inactiveColor;
            Tint(color);

            if (_zoneRing != null)
            {
                _zoneRing.enabled = state == TeleporterState.Charging;
                SetRingColor(_encounter.IsPlayerInZone ? _ringOccupiedColor : _ringEmptyColor);
            }
        }

        private void OnPlayerInZoneChanged(bool occupied)
        {
            SetRingColor(occupied ? _ringOccupiedColor : _ringEmptyColor);
        }

        private void Tint(Color color)
        {
            if (_tintedRenderers == null)
            {
                return;
            }

            _block.SetColor(BaseColorId, color);
            foreach (Renderer tinted in _tintedRenderers)
            {
                if (tinted != null)
                {
                    tinted.SetPropertyBlock(_block);
                }
            }
        }

        private void SetRingColor(Color color)
        {
            if (_zoneRing != null)
            {
                // URP unlit ignores vertex colours, so the colour goes through the material like the tints.
                var block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, color);
                _zoneRing.SetPropertyBlock(block);
            }
        }

        private void BuildRing()
        {
            if (_zoneRing == null)
            {
                return;
            }

            float radius = _encounter.ZoneRadius;
            _zoneRing.useWorldSpace = false;
            _zoneRing.loop = true;
            _zoneRing.positionCount = _ringSegments;
            for (int i = 0; i < _ringSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / _ringSegments;
                _zoneRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius));
            }
        }
    }
}
