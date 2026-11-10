using System;
using UnityEngine;

namespace Vyrden.Progression
{
    /// <summary>
    /// Game flow across the stages of a run: which stage the run is on and whether its objective is done.
    /// Stage objectives (the teleporter) report completion with <see cref="CompleteStage"/>; travelling on
    /// calls <see cref="AdvanceToNextStage"/>. Loading or generating the next stage's level is not handled
    /// here; listeners of <see cref="StageStarted"/> do that. Host-only in multiplayer later.
    /// </summary>
    public class StageProgression : MonoBehaviour
    {
        [Tooltip("Stages in a run. The last one ends with the final boss.")]
        [SerializeField, Min(1)] private int _stageCount = 5;
        [SerializeField, Min(1)] private int _startingStage = 1;

        private int _currentStage;
        private StageState _state = StageState.InProgress;
        private bool _initialized;

        /// <summary>Raised when a stage begins (also the first one). Arg: stage number, 1-based.</summary>
        public event Action<int> StageStarted;

        /// <summary>Raised once when the current stage's objective is completed. Arg: stage number.</summary>
        public event Action<int> StageCompleted;

        public int StageCount => _stageCount;
        public StageState State => _state;
        public bool IsFinalStage => CurrentStage >= _stageCount;

        /// <summary>The objective is done and there is a next stage to go to.</summary>
        public bool CanAdvance => _state == StageState.Completed && !IsFinalStage;

        public int CurrentStage
        {
            get
            {
                EnsureInitialized();
                return _currentStage;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            StageStarted?.Invoke(_currentStage);
        }

        /// <summary>Marks the current stage's objective as done. Returns false if it already was.</summary>
        public bool CompleteStage()
        {
            EnsureInitialized();
            if (_state == StageState.Completed)
            {
                return false;
            }

            _state = StageState.Completed;
            StageCompleted?.Invoke(_currentStage);
            return true;
        }

        /// <summary>Moves to the next stage if the current one is complete. Returns whether it moved.</summary>
        public bool AdvanceToNextStage()
        {
            if (!CanAdvance)
            {
                return false;
            }

            _currentStage++;
            _state = StageState.InProgress;
            StageStarted?.Invoke(_currentStage);
            return true;
        }

        /// <summary>Back to the starting stage (new run).</summary>
        public void ResetProgress()
        {
            _initialized = false;
            EnsureInitialized();
            StageStarted?.Invoke(_currentStage);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _currentStage = Mathf.Clamp(_startingStage, 1, _stageCount);
            _state = StageState.InProgress;
        }
    }
}
