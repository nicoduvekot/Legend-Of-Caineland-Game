using System;

namespace Cutscenes
{
    public interface ICutscene
    {
        bool IsPlaying { get; }
        event Action OnFinish;
    }
}