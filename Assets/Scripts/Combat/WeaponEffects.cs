using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Placeholder visuals for a <see cref="Weapon"/>: muzzle flash, tracer and impact marker.
    /// Only listens to <see cref="Weapon.Fired"/>; never affects damage.
    /// </summary>
    [RequireComponent(typeof(Weapon))]
    public class WeaponEffects : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Material _effectMaterial;

        [Header("Muzzle Flash")]
        [SerializeField] private Renderer _muzzleFlash;
        [SerializeField, Min(0f)] private float _flashDuration = 0.04f;
        [SerializeField] private Color _flashColor = new Color(1f, 0.85f, 0.4f);

        [Header("Tracer")]
        [SerializeField, Min(0f)] private float _tracerDuration = 0.05f;
        [SerializeField, Min(0f)] private float _tracerWidth = 0.03f;
        [SerializeField] private Color _tracerColor = new Color(1f, 0.9f, 0.6f);

        [Header("Impact")]
        [SerializeField, Min(0f)] private float _impactDuration = 0.15f;
        [SerializeField, Min(0f)] private float _impactSize = 0.2f;
        [SerializeField] private Color _missColor = new Color(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color _hitColor = new Color(1f, 0.3f, 0.2f);
        [SerializeField] private Color _killColor = new Color(1f, 0.1f, 0.1f);

        [Header("Pooling")]
        [SerializeField, Min(1)] private int _poolSize = 12;

        private sealed class Fx
        {
            public GameObject GameObject;
            public Renderer Renderer;
            public LineRenderer Line;
            public float TimeLeft;
            public float Duration;
        }

        private readonly List<Fx> _tracers = new List<Fx>();
        private readonly List<Fx> _impacts = new List<Fx>();
        private MaterialPropertyBlock _block;
        private Weapon _weapon;
        private Transform _poolRoot;
        private float _flashTimeLeft;
        private int _nextTracer;
        private int _nextImpact;

        private void Awake()
        {
            _weapon = GetComponent<Weapon>();
            _block = new MaterialPropertyBlock();

            if (_effectMaterial == null)
            {
                Debug.LogWarning($"{nameof(WeaponEffects)} on '{name}' has no effect material; effects are disabled.", this);
                enabled = false;
                return;
            }

            _poolRoot = new GameObject($"{name}_EffectsPool").transform;
            for (int i = 0; i < _poolSize; i++)
            {
                _tracers.Add(CreateTracer());
                _impacts.Add(CreateImpact());
            }

            if (_muzzleFlash != null)
            {
                SetColor(_muzzleFlash, _flashColor);
                _muzzleFlash.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_weapon != null)
            {
                _weapon.Fired += OnFired;
            }
        }

        private void OnDisable()
        {
            if (_weapon != null)
            {
                _weapon.Fired -= OnFired;
            }
        }

        private void OnDestroy()
        {
            if (_poolRoot != null)
            {
                Destroy(_poolRoot.gameObject);
            }
        }

        private void OnFired(ShotResult shot)
        {
            if (_muzzleFlash != null)
            {
                _muzzleFlash.enabled = true;
                _flashTimeLeft = _flashDuration;
            }

            Fx tracer = _tracers[_nextTracer];
            _nextTracer = (_nextTracer + 1) % _tracers.Count;
            tracer.Line.SetPosition(0, shot.Origin);
            tracer.Line.SetPosition(1, shot.EndPoint);
            Activate(tracer, _tracerDuration);

            if (!shot.HitSomething)
            {
                return;
            }

            Fx impact = _impacts[_nextImpact];
            _nextImpact = (_nextImpact + 1) % _impacts.Count;
            impact.GameObject.transform.position = shot.EndPoint + shot.HitNormal * 0.02f;
            Color color = shot.Killed ? _killColor : shot.HitDamageable ? _hitColor : _missColor;
            SetColor(impact.Renderer, color);
            Activate(impact, _impactDuration);
        }

        private void Update()
        {
            if (_muzzleFlash != null && _muzzleFlash.enabled)
            {
                _flashTimeLeft -= Time.deltaTime;
                _muzzleFlash.enabled = _flashTimeLeft > 0f;
            }

            float deltaTime = Time.deltaTime;
            for (int i = 0; i < _tracers.Count; i++)
            {
                Fx tracer = _tracers[i];
                if (Tick(tracer, deltaTime, out float t))
                {
                    tracer.Line.widthMultiplier = _tracerWidth * t;
                }

                Fx impact = _impacts[i];
                if (Tick(impact, deltaTime, out t))
                {
                    impact.GameObject.transform.localScale = Vector3.one * (_impactSize * t);
                }
            }
        }

        private static bool Tick(Fx fx, float deltaTime, out float remaining)
        {
            remaining = 0f;
            if (!fx.GameObject.activeSelf)
            {
                return false;
            }

            fx.TimeLeft -= deltaTime;
            if (fx.TimeLeft <= 0f)
            {
                fx.GameObject.SetActive(false);
                return false;
            }

            remaining = fx.TimeLeft / fx.Duration;
            return true;
        }

        private static void Activate(Fx fx, float duration)
        {
            fx.Duration = Mathf.Max(duration, 0.0001f);
            fx.TimeLeft = fx.Duration;
            fx.GameObject.SetActive(true);
        }

        private Fx CreateTracer()
        {
            var go = new GameObject("Tracer");
            go.transform.SetParent(_poolRoot, false);
            var line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.sharedMaterial = _effectMaterial;
            line.widthMultiplier = _tracerWidth;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            SetColor(line, _tracerColor);
            go.SetActive(false);
            return new Fx { GameObject = go, Renderer = line, Line = line };
        }

        private Fx CreateImpact()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Impact";
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(_poolRoot, false);
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = _effectMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.SetActive(false);
            return new Fx { GameObject = go, Renderer = renderer };
        }

        private void SetColor(Renderer target, Color color)
        {
            target.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            target.SetPropertyBlock(_block);
        }
    }
}
