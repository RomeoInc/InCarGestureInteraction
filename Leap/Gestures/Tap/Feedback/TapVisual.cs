using System;
using Leap;
using Interfaces;
using Leap.Gestures.Pointing;

namespace Leap.Gestures.Tap.Feedback
{
    public class TapVisual : NetworkInterface, IPointingObserver, ITapObserver
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
        
        private bool skip;

        public TapVisual(String name)
            : base(name)
        {
            skip = false;
        }

        #region IPointingTapObserver
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

            Send(String.Format("{0} {1:0.00} {2:0.00} {3:0.00} 0", MessageCursorUpdate, pos.x, pos.y, pos.z));
        }
        #endregion

        #region ISelectionTapObserver
        public void EnterTarget(Vector pos, ROI.ROI roi)
        {
            Send(String.Format("{0} {1} {2} {3}", MessageDwellStart, roi.Name, pos.x, pos.z));
        }

        public void LeaveTarget()
        {
            Send(String.Format("{0}", MessageDwellStop));
        }

        public void TapSelect(Vector pos, ROI.ROI roi, DateTime time)
        {
            Send(String.Format("{0} {1}", MessageDwellSelect, roi.Name));
        }

        public void ProgressUpdate(double progress)
        {
            Send(String.Format("{0} {1}", MessageProgressUpdate, (int)(progress*360)));
        }
        #endregion

        public new void Send(String message)
        {
            if (base.Connected)
                base.Send(message);
        }
    }
}
