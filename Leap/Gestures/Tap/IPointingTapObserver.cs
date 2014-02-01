using System;

namespace Leap.Gestures.Tap
{
    public interface IPointingTapObserver
    {
        void EnterWorkspace();

        void LeaveWorkspace();

        void InitialPose();

        void ExitPose();

        void StillPointing(Vector pos, DateTime when);
    }
}
