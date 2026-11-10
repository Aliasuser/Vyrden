using System;
using UnityEngine;

namespace Vyrden.Core
{
    /// <summary>
    /// Elapsed time of the current run. Counts scaled time, so it also stops whenever the game is paused
    /// through <see cref="Time.timeScale"/>; <see cref="Pause"/> / <see cref="Resume"/> stop it explicitly
    /// (pause menu, cutscenes). Once <see cref="EndRun"/> is called the time is frozen until the next
    /// <see cref="StartRun"/>. The host owns this in multiplayer later; clients would only display it.
    /// </summary>
    [DefaultExecutionOrder(-60)] // Before anything that reads the time this frame (e.g. the difficulty director).
    public class RunTimer : MonoBehaviour
    {
        [Tooltip("Start the run automatically when the scene starts.")]
        [SerializeField] private bool _startOnStart = true;

        private RunState _state = RunState.NotStarted;
        private float _elapsedTime;

        public event Action RunStarted;
        public event Action Paused;
        public event Action Resumed;

        /// <summary>Raised once when the run ends. Arg: final elapsed time in seconds.</summary>
        public event Action<float> RunEnded;

        public RunState State => _state;
        public bool IsRunning => _state == RunState.Running;

        /// <summary>Seconds of run time so far (excludes paused time).</summary>
        public float ElapsedTime => _elapsedTime;

        private void Start()
        {
            if (_startOnStart && _state == RunState.NotStarted)
            {
                StartRun();
            }
        }

        private void Update()
        {
            if (_state == RunState.Running)
            {
                _elapsedTime += Time.deltaTime;
            }
        }

        /// <summary>Starts a new run from zero, also after a previous run ended.</summary>
        public void StartRun()
        {
            _elapsedTime = 0f;
            _state = RunState.Running;
            RunStarted?.Invoke();
        }

        public void Pause()
        {
            if (_state != RunState.Running)
            {
                return;
            }

            _state = RunState.Paused;
            Paused?.Invoke();
        }

        public void Resume()
        {
            if (_state != RunState.Paused)
            {
                return;
            }

            _state = RunState.Running;
            Resumed?.Invoke();
        }

        /// <summary>Freezes the time. Does nothing if the run is not in progress.</summary>
        public void EndRun()
        {
            if (_state != RunState.Running && _state != RunState.Paused)
            {
                return;
            }

            _state = RunState.Ended;
            RunEnded?.Invoke(_elapsedTime);
        }

        /// <summary>Debug and save-restore hook: sets the clock to a given time without changing the state.</summary>
        public void SetElapsedTime(float seconds)
        {
            _elapsedTime = Mathf.Max(0f, seconds);
        }
    }
}
