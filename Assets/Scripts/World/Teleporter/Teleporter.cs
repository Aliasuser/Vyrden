using UnityEngine;
using Vyrden.Core;
using Vyrden.Progression;

namespace Vyrden.World
{
    /// <summary>
    /// The interaction side of the teleporter. Inactive: interacting starts the encounter. Charged:
    /// interacting travels to the next stage through <see cref="StageProgression"/>. Holds no encounter or
    /// stage rules itself.
    /// </summary>
    [RequireComponent(typeof(TeleporterEncounter))]
    public class Teleporter : MonoBehaviour, IInteractable
    {
        [Tooltip("Where interaction distance is measured to. Defaults to this transform.")]
        [SerializeField] private Transform _interactionPoint;

        private TeleporterEncounter _encounter;

        public Vector3 InteractionPoint => _interactionPoint != null ? _interactionPoint.position : transform.position;
        public TeleporterEncounter Encounter => _encounter;

        private void Awake()
        {
            _encounter = GetComponent<TeleporterEncounter>();
        }

        public bool CanInteract(GameObject interactor)
        {
            switch (_encounter.State)
            {
                case TeleporterState.Inactive:
                    return _encounter.enabled;
                case TeleporterState.Charged:
                    return _encounter.StageProgression != null && _encounter.StageProgression.CanAdvance;
                default:
                    return false;
            }
        }

        public string GetPrompt(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return string.Empty;
            }

            return _encounter.State == TeleporterState.Inactive
                ? "Activate teleporter"
                : $"Travel to stage {_encounter.StageProgression.CurrentStage + 1}";
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            if (_encounter.State == TeleporterState.Inactive)
            {
                _encounter.Begin();
            }
            else
            {
                _encounter.StageProgression.AdvanceToNextStage();
            }
        }
    }
}
