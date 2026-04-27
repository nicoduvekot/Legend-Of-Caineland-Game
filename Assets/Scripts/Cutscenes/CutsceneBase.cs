using System;
using UnityEngine;

namespace Cutscenes
{
    /// <summary>
    /// A base class to be used by cutscene elements that need to play
    ///
    /// NOTE: for now, this is just setup to work at the START of a level.
    /// Mid-level or technically end-level may not work as intended.
    /// </summary>
    public abstract class CutsceneBase : MonoBehaviour, ICutscene
    {
        public bool IsPlaying { get; private set; }
        public event Action OnFinish;
        
        /// <summary>
        /// Call this to begin the cutscene
        /// Derived classes should override OnStartCutscene() for logic
        /// </summary>
        public void Play()
        {
            if (IsPlaying)
                return;

            IsPlaying = true;
            OnStartCutscene();
        }
        
        /// <summary>
        /// Derived classes use this for their cutscene logic
        /// MUST call EndCutscene() when finished
        /// </summary>
        protected abstract void OnStartCutscene();
        
        /// <summary>
        /// Call this when cutscene is over
        /// Fires the event and marks cutscene as over
        /// </summary>
        protected void EndCutscene()
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;
            OnFinish?.Invoke();
        }
    }
}