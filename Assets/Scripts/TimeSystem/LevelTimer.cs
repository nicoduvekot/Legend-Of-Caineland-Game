using TMPro;
using UnityEngine;
using Utilities;

namespace TimeSystem
{
    public sealed class LevelTimer : PersistentSingleton<LevelTimer>
    {
        [SerializeField] private GameObject timerRoot; // root for enable / disable
        [SerializeField] private TextMeshProUGUI timeLabel; // actual label display

        private bool _isRunning;
        private float _elapsedSeconds;
        
        // unity functions

        protected override void Awake()
        {
            base.Awake();
            UpdateLabel();
            HideTimer();
        }

        private void Update()
        {
            if (!_isRunning) return;
            
            _elapsedSeconds += Time.deltaTime;
            UpdateLabel();
        }
        
        // public API

        /// <summary>
        /// Enables the visual for the timer
        /// </summary>
        public void ShowTimer() => timerRoot.SetActive(true);

        /// <summary>
        /// Disables the visual for the timer
        /// </summary>
        public void HideTimer() => timerRoot.SetActive(false);

        /// <summary>
        /// Starts timer ticking again - no reset of elapsed time
        /// </summary>
        public void StartTimer() => _isRunning = true;

        /// <summary>
        /// Stops timer ticking - no reset of elapsed time
        /// </summary>
        public void StopTimer() => _isRunning = false;
        
        /// <summary>
        /// Returns the elapsed time as a float for seconds
        /// </summary>
        /// <returns></returns>
        public float ReportTime() => _elapsedSeconds;

        /// <summary>
        /// Resets the elapsed time of the timer
        /// </summary>
        public void ResetTimer()
        {
            _elapsedSeconds = 0.0f;
            UpdateLabel();
        }
        
        /// <summary>
        /// Should be used upon load to set the current elapsed time
        /// </summary>
        /// <param name="seconds"></param>
        public void SetElapsedSeconds(float seconds)
        {
            _elapsedSeconds = seconds;
            UpdateLabel();
        }
        
        // private helper
        
        private void UpdateLabel()
        {
            int minutes = (int)(_elapsedSeconds / 60f);
            int seconds = (int)(_elapsedSeconds % 60f);
            
            timeLabel.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
