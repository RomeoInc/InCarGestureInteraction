using System;

namespace Leap.Gestures.Tap
{
    public interface ITapObserver
    {
        void EnterTarget(Vector pos, ROI.ROI roi);

        void LeaveTarget();

        void TapSelect(Vector pos, ROI.ROI roi, DateTime time);

        void ProgressUpdate(double progress);
    }
}