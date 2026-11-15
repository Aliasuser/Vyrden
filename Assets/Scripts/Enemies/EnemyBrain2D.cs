using System;
using UnityEngine;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Enemies
{
    /// <summary>
    /// 2D enemy AI on the XY plane: Idle → Alerted → Chase → Attack → Recover, and Dead. Detects hostile
    /// <see cref="TeamMember"/>s by distance, moves only through <see cref="CharacterMotor2D.SetIntent"/> and
    /// attacks only through the enemy's <see cref="IAttack"/> (melee or ranged), so enemy types differ by
    /// data and components, not by brain code. Knows nothing about sprites or animation: presentation
    /// reads <see cref="State"/>, <see cref="StateChanged"/> and <see cref="FacingDirection"/>.
    /// Optional room bounds keep detection and steering inside one room.
    /// </summary>
    [RequireComponent(typeof(Enemy), typeof(CharacterMotor2D))]
    [DisallowMultipleComponent]
    public class EnemyBrain2D : MonoBehaviour, IRoomBoundsReceiver
    {
        [Tooltip("Stop moving at this fraction of attack range so the enemy doesn't push into the target.")]
        [SerializeField, Range(0.1f, 1f)] private float _stopDistanceFactor = 0.8f;

        [Header("Room bounds (optional)")]
        [Tooltip("When on, only targets inside the bounds are noticed and the enemy never steers towards a point outside them.")]
        [SerializeField] private bool _useRoomBounds;
        [SerializeField] private Rect _roomBounds = new Rect(-7f, -4f, 14f, 8f);
        [Tooltip("How far inside the bounds steering targets are kept (about the enemy's radius).")]
        [SerializeField, Min(0f)] private float _boundsMargin = 0.3f;

        private Enemy _enemy;
        private CharacterMotor2D _motor;
        private TeamMember _self;
        private IAttack _attack;
        private EnemyState _state = EnemyState.Idle;
        private float _stateTimer;
        private TeamMember _target;
        private Vector2 _facing = Vector2.down;

        public event Action<EnemyState> StateChanged;

        public EnemyState State => _state;
        public TeamMember Target => _target;

        /// <summary>Where the enemy is looking: at its target while engaged, otherwise its movement facing.</summary>
        public Vector2 FacingDirection => _facing;

        public bool HasRoomBounds => _useRoomBounds;
        public Rect RoomBounds => _roomBounds;

        private EnemyDefinition Definition => _enemy.Definition;
        private Vector2 Position => new Vector2(transform.position.x, transform.position.y);

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _motor = GetComponent<CharacterMotor2D>();
            _self = GetComponent<TeamMember>();
        }

        private void Start()
        {
            // Enemy finds its attack in Awake; read it after every Awake has run.
            _attack = _enemy.AttackBehaviour;
            if (_attack == null)
            {
                Debug.LogWarning($"{nameof(EnemyBrain2D)} on '{name}' has no attack; it will only chase.", this);
            }
        }

        private void OnEnable()
        {
            _enemy.Health.Died += OnDied;
        }

        private void OnDisable()
        {
            _enemy.Health.Died -= OnDied;
            if (_motor != null)
            {
                _motor.SetIntent(default);
            }
        }

        /// <summary>Limits the enemy to a room (set by the room when it activates its enemies).</summary>
        public void SetRoomBounds(Rect bounds)
        {
            _roomBounds = bounds;
            _useRoomBounds = true;
        }

        public void ClearRoomBounds()
        {
            _useRoomBounds = false;
        }

        private void Update()
        {
            if (_state == EnemyState.Dead || Definition == null)
            {
                return;
            }

            switch (_state)
            {
                case EnemyState.Idle: UpdateIdle(); break;
                case EnemyState.Alerted: UpdateAlerted(); break;
                case EnemyState.Chase: UpdateChase(); break;
                case EnemyState.Attack: UpdateAttack(); break;
                case EnemyState.Recover: UpdateRecover(); break;
            }
        }

        private void UpdateIdle()
        {
            StandStill();
            _facing = _motor.Facing;

            TeamMember candidate = TeamMember.FindClosestHostile(_self.Team, transform.position, Definition.DetectionRange);
            if (candidate != null && IsInsideRoom(candidate.transform.position))
            {
                _target = candidate;
                SetState(EnemyState.Alerted, Definition.ReactionTime);
            }
        }

        private void UpdateAlerted()
        {
            if (!HasValidTarget())
            {
                LoseTarget();
                return;
            }

            StandStill();
            FaceTarget();

            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                SetState(EnemyState.Chase);
            }
        }

        private void UpdateChase()
        {
            if (!HasValidTarget())
            {
                LoseTarget();
                return;
            }

            FaceTarget();
            if (_attack != null && _attack.TryStartAttack(_target.transform))
            {
                StandStill();
                SetState(EnemyState.Attack);
                return;
            }

            // Hold position once close enough and able to attack (in range, and for ranged attacks with
            // a clear line); otherwise keep closing in, which also walks around to find a line of sight.
            float range = _attack != null ? _attack.Range : Definition.AttackRange;
            bool holdPosition = DistanceToTarget() <= range * _stopDistanceFactor
                && (_attack == null || _attack.CanReach(_target.transform));

            if (holdPosition)
            {
                StandStill();
                return;
            }

            Vector2 goal = ClampToRoom(new Vector2(_target.transform.position.x, _target.transform.position.y));
            Vector2 toGoal = goal - Position;
            Move(toGoal.sqrMagnitude > 0.0004f ? toGoal.normalized : Vector2.zero);
        }

        private void UpdateAttack()
        {
            StandStill();
            if (_target != null)
            {
                FaceTarget();
            }

            if (_attack == null || !_attack.IsAttacking)
            {
                SetState(EnemyState.Recover, Definition.RecoverDuration);
            }
        }

        private void UpdateRecover()
        {
            StandStill();

            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                SetState(EnemyState.Chase);
            }
        }

        private void OnDied(DamageInfo killingBlow)
        {
            _attack?.Cancel();
            StandStill();
            _target = null;
            SetState(EnemyState.Dead);
        }

        private bool HasValidTarget()
        {
            if (_target == null || !_target.isActiveAndEnabled || !_target.IsAlive || !IsInsideRoom(_target.transform.position))
            {
                return false;
            }

            float range = Definition.LoseTargetRange;
            return (WorldToPlanar(_target.transform.position) - Position).sqrMagnitude <= range * range;
        }

        private void LoseTarget()
        {
            _target = null;
            SetState(EnemyState.Idle);
        }

        private bool IsInsideRoom(Vector3 worldPosition)
        {
            return !_useRoomBounds || _roomBounds.Contains(WorldToPlanar(worldPosition));
        }

        private Vector2 ClampToRoom(Vector2 point)
        {
            if (!_useRoomBounds)
            {
                return point;
            }

            float margin = Mathf.Min(_boundsMargin, _roomBounds.width * 0.5f, _roomBounds.height * 0.5f);
            return new Vector2(
                Mathf.Clamp(point.x, _roomBounds.xMin + margin, _roomBounds.xMax - margin),
                Mathf.Clamp(point.y, _roomBounds.yMin + margin, _roomBounds.yMax - margin));
        }

        private float DistanceToTarget()
        {
            return (WorldToPlanar(_target.transform.position) - Position).magnitude;
        }

        private void FaceTarget()
        {
            Vector2 offset = WorldToPlanar(_target.transform.position) - Position;
            if (offset.sqrMagnitude > 1e-6f)
            {
                _facing = offset.normalized;
            }
        }

        private void Move(Vector2 direction)
        {
            _motor.SetIntent(new MovementIntent { MoveDirection = new Vector3(direction.x, direction.y, 0f) });
        }

        private void StandStill()
        {
            _motor.SetIntent(default);
        }

        private void SetState(EnemyState state, float timer = 0f)
        {
            _stateTimer = timer;
            if (_state == state)
            {
                return;
            }

            _state = state;
            StateChanged?.Invoke(state);
        }

        private static Vector2 WorldToPlanar(Vector3 world)
        {
            return new Vector2(world.x, world.y);
        }
    }
}
