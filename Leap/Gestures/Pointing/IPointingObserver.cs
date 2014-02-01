using System;

namespace Leap.Gestures.Pointing
{
    public interface IPointingObserver
    {
        void EnterWorkspace();

        void LeaveWorkspace();

        void InitialPose();

        void ExitPose();

        void StillPointing(Vector pos, DateTime when);
    }
}
