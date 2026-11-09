using System;
using UnityEngine;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Enemies
{
    /// <summary>
    /// Decides what the enemy does. Moves only through <see cref="CharacterMotor.SetIntent"/> and attacks
    /// only through <see cref="MeleeAttack"/>; holds no movement or damage logic itself.
    /// </summary>
    [RequireComponent(typeof(Enemy), typeof(CharacterMotor))]
    public class EnemyBrain : MonoBehaviour
    {
        [Tooltip("Stop moving at this fraction of attack range so the enemy doesn't push into the target.")]
        [SerializeField, Range(0.1f, 1f)] private float _stopDistanceFactor = 0.8f;

        private Enemy _enemy;
        private CharacterMotor _motor;
        private TeamMember _self;
        private EnemyState _state = EnemyState.Idle;
        private float _stateTimer;
        private TeamMember _target;

        public event Action<EnemyState> StateChanged;

        public EnemyState State => _state;
        public TeamMember Target => _target;

        private EnemyDefinition Definition => _enemy.Definition;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _motor = GetComponent<CharacterMotor>();
            _self = GetComponent<TeamMember>();
        }

        private void OnEnable()
        {
            _enemy.Health.Died += OnDied;
        }

        private void OnDisable()
        {
            _enemy.Health.Died -= OnDied;
        }

        private void Update()
        {
            if (_state == EnemyState.Dead || Definition == null)
            {
                return;
            }

            switch (_state)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;
                case EnemyState.Alerted:
                    UpdateAlerted();
                    break;
                case EnemyState.Chase:
                    UpdateChase();
                    break;
                case EnemyState.Attack:
                    UpdateAttack();
                    break;
                case EnemyState.Recover:
                    UpdateRecover();
                    break;
            }
        }

        private void UpdateIdle()
        {
            StandStill(Vector3.zero);

            _target = TeamMember.FindClosestHostile(_self.Team, transform.position, Definition.DetectionRange);
            if (_target != null)
            {
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

            StandStill(DirectionToTarget());

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

            MeleeAttack attack = _enemy.Attack;
            if (attack != null && attack.TryStartAttack(_target.transform))
            {
                StandStill(DirectionToTarget());
                SetState(EnemyState.Attack);
                return;
            }

            Vector3 toTarget = DirectionToTarget();
            float stopDistance = (attack != null ? attack.Range : Definition.AttackRange) * _stopDistanceFactor;
            bool closeEnough = FlatDistanceToTarget() <= stopDistance;

            _motor.SetIntent(new MovementIntent
            {
                MoveDirection = closeEnough ? Vector3.zero : toTarget,
                FaceDirection = toTarget,
            });
        }

        private void UpdateAttack()
        {
            StandStill(_target != null ? DirectionToTarget() : Vector3.zero);

            if (_enemy.Attack == null || !_enemy.Attack.IsAttacking)
            {
                SetState(EnemyState.Recover, Definition.RecoverDuration);
            }
        }

        private void UpdateRecover()
        {
            StandStill(_target != null ? DirectionToTarget() : Vector3.zero);

            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                SetState(EnemyState.Chase);
            }
        }

        private void OnDied(DamageInfo killingBlow)
        {
            _enemy.Attack?.Cancel();
            StandStill(Vector3.zero);
            _target = null;
            SetState(EnemyState.Dead);
        }

        private bool HasValidTarget()
        {
            if (_target == null || !_target.isActiveAndEnabled || !_target.IsAlive)
            {
                return false;
            }

            float range = Definition.LoseTargetRange;
            return (_target.transform.position - transform.position).sqrMagnitude <= range * range;
        }

        private void LoseTarget()
        {
            _target = null;
            SetState(EnemyState.Idle);
        }

        private Vector3 DirectionToTarget()
        {
            Vector3 offset = _target.transform.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude > 0.0001f ? offset.normalized : Vector3.zero;
        }

        private float FlatDistanceToTarget()
        {
            Vector3 offset = _target.transform.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void StandStill(Vector3 faceDirection)
        {
            _motor.SetIntent(new MovementIntent { FaceDirection = faceDirection });
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
    }
}
