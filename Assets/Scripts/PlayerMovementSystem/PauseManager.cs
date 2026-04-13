using System;
using UnityEngine;
using Utilities;

namespace PlayerMovementSystem
{
    public class PauseManager : PersistentSingleton<PauseManager>
    {
        public static bool IsPaused { get; private set; }

        public static event Action OnPaused;
        public static event Action OnUnpaused;

        public void TogglePause()
        {
            if (IsPaused)
                Unpause();
            else
                Pause();
        }

        private static void Pause()
        {
            if (IsPaused) return;
            
            IsPaused = true;
            Time.timeScale = 0f;
            
            OnPaused?.Invoke();
        }

        private static void Unpause()
        {
            if (!IsPaused) return;
            
            IsPaused = false;
            Time.timeScale = 1f;
            
            OnUnpaused?.Invoke();
        }
    }
}