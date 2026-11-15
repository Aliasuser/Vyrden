using UnityEngine;
using UnityEngine.InputSystem;
using Vyrden.Combat;
using Vyrden.Core;
using Vyrden.Movement;

namespace Vyrden.Player
{
    /// <summary>
    /// Input for the 2D top-down player: reads the Move action (WASD / left stick) and hands a
    /// <see cref="MovementIntent"/> on the XY plane to the <see cref="CharacterMotor2D"/>. The camera is fixed,
    /// so W is always screen-up. Also reads the AimPoint action (pointer position), turns it into a point on
    /// the XY plane through the gameplay camera and hands it to <see cref="CharacterAim2D"/>; movement and aim
    /// are separate and never feed each other. No input reaches either while the player is dead.
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
        [SerializeField] private string _aimPointActionName = "AimPoint";

        [Header("Targets")]
        [Tooltip("Defaults to the CharacterMotor2D on this object.")]
        [SerializeField] private CharacterMotor2D _motor;
        [Tooltip("Defaults to the CharacterAim2D on this object. Optional: without it there is no aiming.")]
        [SerializeField] private CharacterAim2D _aim;
        [Tooltip("Camera used to turn the pointer into a world point. Defaults to the MainCamera-tagged camera.")]
        [SerializeField] private Camera _aimCamera;

        private InputActionMap _actionMap;
        private InputAction _move;
        private InputAction _aimPoint;
        private Health _health;

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponent<CharacterMotor2D>();
            }

            if (_aim == null)
            {
                _aim = GetComponent<CharacterAim2D>();
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

            if (_aim != null)
            {
                _aimPoint = _actionMap.FindAction(_aimPointActionName, true);
                if (_aimCamera == null)
                {
                    _aimCamera = Camera.main;
                }

                if (_aimCamera == null)
                {
                    Debug.LogError($"{nameof(PlayerInputReader2D)} on '{name}' has a {nameof(CharacterAim2D)} but no aim camera.", this);
                }
            }
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

            if (canAct)
            {
                UpdateAim();
            }
        }

        private void UpdateAim()
        {
            // No pointer driving the action (gamepad only, or the device was reset when the window lost
            // focus): its value reads (0, 0), which would snap the aim to the bottom-left corner. Keep the last aim.
            if (_aim == null || _aimCamera == null || _aimPoint.activeControl == null)
            {
                return;
            }

            // Re-projected every frame, not cached in world space: the camera and the player move while
            // the pointer stays still, and the aim must follow what is under the cursor now.
            Vector2 screen = AimMath.ClampToRect(_aimPoint.ReadValue<Vector2>(), _aimCamera.pixelRect);
            Ray ray = _aimCamera.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
            if (AimMath.TryIntersectPlane(ray, WorldPlane.GameplayZ, out Vector2 worldPoint))
            {
                _aim.SetAimPoint(worldPoint);
            }
        }
    }
}
