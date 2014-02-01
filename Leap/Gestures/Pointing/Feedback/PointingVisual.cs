using System;
using Leap;
using Interfaces;

namespace Leap.Gestures.Pointing.Feedback
{
    public class PointingVisual : NetworkInterface, IPointingObserver, ISelectionObserver
    {
        public const String MessageEnter = "1";
        public const String MessageLeave = "2";
        public const String MessagePoseStart = "3";
        public const String MessagePoseEnd = "4";
        public const String MessageDwellStart = "5";
        public const String MessageDwellStop = "6";
        public const String MessageDwellSelect = "7";
        public const String MessageCursorUpdate = "8";
        public const String MessageProgressUpdate = "9";

        private bool skip = false;

        public PointingVisual(String name)
            : base(name)
        {

        }

        #region IPointingObserver
        public void EnterWorkspace()
        {
            Send(MessageEnter);
        }

        public void LeaveWorkspace()
        {
            Send(MessageLeave);
        }

        public void InitialPose()
        {
            Send(MessagePoseStart);
        }

        public void ExitPose()
        {
            Send(MessagePoseEnd);
        }

        public void StillPointing(Vector pos, DateTime when)
        {
            if ((skip = !skip))
                return;

            Send(String.Format("{0} {1:0.00} {2:0.00} {3:0.00} 0 0", MessageCursorUpdate, pos.x, pos.y, pos.z));
        }
        #endregion

        #region IDwellSelectionObserver
        public void StartDwell(Vector pos, ROI.ROI roi)
        {
            Send(String.Format("{0} {1} {2} {3}", MessageDwellStart, roi.Name, pos.x, pos.z));
        }

        public void StopDwell()
        {
            Send(String.Format("{0}", MessageDwellStop));
        }

        public void DwellSelect(Vector pos, ROI.ROI roi, DateTime time)
        {
            Send(String.Format("{0} {1}", MessageDwellSelect, roi.Name));
        }

        public void ProgressUpdate(long dwellTime)
        {
            int progress = dwellTime < PointingDetector.MIN_TIME ? 0
                : (int)((dwellTime - PointingDetector.MIN_TIME) / (float)(PointingDetector.DWELL - PointingDetector.MIN_TIME) * 360);

            Send(String.Format("{0} {1}", MessageProgressUpdate, progress));
        }
        #endregion

        public new void Send(String message)
        {
            if (base.Connected)
                base.Send(message);
        }
    }
}
