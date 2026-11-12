using UnityEngine;
using UnityEngine.InputSystem;
using Vyrden.Combat;
using Vyrden.Movement;

namespace Vyrden.Player
{
    /// <summary>
    /// Input for the 2D top-down player: reads the Move action (WASD / left stick) and hands a
    /// <see cref="MovementIntent"/> on the XY plane to the <see cref="CharacterMotor2D"/>. The camera is fixed,
    /// so W is always screen-up. No input reaches the motor while the player is dead. Aiming comes later.
    /// The 3D prototype keeps using <see cref="PlayerInputReader"/>.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader2D : MonoBehaviour
    {
        [Header("Input Actions")]
        [Tooltip("Leave empty to use the project-wide actions.")]
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _actionMapName = "Player";
        [SerializeField] private string _moveActionName = "Move";

        [Header("Targets")]
        [Tooltip("Defaults to the CharacterMotor2D on this object.")]
        [SerializeField] private CharacterMotor2D _motor;

        private InputActionMap _actionMap;
        private InputAction _move;
        private Health _health;

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponent<CharacterMotor2D>();
            }

            _health = GetComponent<Health>();

            InputActionAsset actions = _actions != null ? _actions : InputSystem.actions;
            _actionMap = actions != null ? actions.FindActionMap(_actionMapName) : null;
            if (_actionMap == null || _motor == null)
            {
                Debug.LogError($"{nameof(PlayerInputReader2D)} on '{name}' needs action map '{_actionMapName}' and a {nameof(CharacterMotor2D)}.", this);
                enabled = false;
                return;
            }

            _move = _actionMap.FindAction(_moveActionName, true);
        }

        private void OnEnable()
        {
            _actionMap?.Enable();
        }

        private void OnDisable()
        {
            if (_motor != null)
            {
                _motor.SetIntent(default);
            }
        }

        private void Update()
        {
            bool canAct = _health == null || _health.IsAlive;
            Vector2 move = canAct ? Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f) : Vector2.zero;
            _motor.SetIntent(new MovementIntent { MoveDirection = new Vector3(move.x, move.y, 0f) });
        }
    }
}
