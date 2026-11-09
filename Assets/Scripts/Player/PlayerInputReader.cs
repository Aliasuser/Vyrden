using UnityEngine;
using UnityEngine.InputSystem;

namespace Vyrden.Player
{
    /// <summary>
    /// The only player script that touches the Input System. Turns input into a <see cref="MovementIntent"/>
    /// for the motor and look input for the camera.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [Tooltip("Leave empty to use the project-wide actions.")]
        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _actionMapName = "Player";
        [SerializeField] private string _moveActionName = "Move";
        [SerializeField] private string _lookActionName = "Look";
        [SerializeField] private string _jumpActionName = "Jump";
        [SerializeField] private string _sprintActionName = "Sprint";

        [Header("Targets")]
        [SerializeField] private CharacterMotor _motor;
        [Tooltip("Leave empty to use the ThirdPersonCamera on the main camera.")]
        [SerializeField] private ThirdPersonCamera _camera;

        [Header("Cursor")]
        [Tooltip("Lock the cursor while playing. Escape releases it, clicking locks it again.")]
        [SerializeField] private bool _lockCursor = true;

        private InputActionMap _actionMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _sprint;

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponent<CharacterMotor>();
            }

            InputActionAsset actions = _actions != null ? _actions : InputSystem.actions;
            _actionMap = actions != null ? actions.FindActionMap(_actionMapName) : null;

            if (_actionMap == null)
            {
                Debug.LogError($"{nameof(PlayerInputReader)} on '{name}' could not find action map '{_actionMapName}'.", this);
                enabled = false;
                return;
            }

            _move = _actionMap.FindAction(_moveActionName, true);
            _look = _actionMap.FindAction(_lookActionName, true);
            _jump = _actionMap.FindAction(_jumpActionName, true);
            _sprint = _actionMap.FindAction(_sprintActionName, true);
        }

        private void Start()
        {
            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main.GetComponent<ThirdPersonCamera>();
            }
        }

        private void OnEnable()
        {
            _actionMap?.Enable();
            SetCursorLocked(_lockCursor);
        }

        private void OnDisable()
        {
            _actionMap?.Disable();
            SetCursorLocked(false);
        }

        private void Update()
        {
            UpdateCursorLock();
            ApplyLookInput();

            if (_motor != null)
            {
                _motor.SetIntent(BuildMovementIntent());
            }
        }

        private void ApplyLookInput()
        {
            if (_camera == null || (_lockCursor && Cursor.lockState != CursorLockMode.Locked))
            {
                return;
            }

            Vector2 look = _look.ReadValue<Vector2>();
            if (_look.activeControl?.device is Pointer)
            {
                _camera.AddLookDelta(look);
            }
            else
            {
                _camera.AddLookRate(look);
            }
        }

        private MovementIntent BuildMovementIntent()
        {
            Vector2 move = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);

            Vector3 forward = _camera != null ? _camera.PlanarForward : Vector3.forward;
            Vector3 right = _camera != null ? _camera.PlanarRight : Vector3.right;

            return new MovementIntent
            {
                MoveDirection = forward * move.y + right * move.x,
                Sprint = _sprint.IsPressed(),
                JumpRequested = _jump.WasPressedThisFrame(),
            };
        }

        private void UpdateCursorLock()
        {
            if (!_lockCursor)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
