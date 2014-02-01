using System;
using Interfaces;

namespace Leap.Gestures.Count.Feedback
{
    class CountVisual : NetworkInterface, ICountObserver
    {
        public const String MessageEnter = "h";
        public const String MessageLeave = "i";
        public const String MessageCountStart = "j";
        public const String MessageCountStop = "k";
        public const String MessageCountSelect = "l";
        public const String MessageCountUpdate = "m";
        public const String MessageCountProgress = "n";
        public const String MessageGroupLeave = "o";
        public const String MessageGroupEnter = "p";
        public const String MessageBack = "q";

        public CountVisual(String name)
            : base(name)
        {

        }

        public void EnterWorkspace(int hands, int fingers)
        {
            Send(MessageEnter);
        }

        public void LeaveWorkspace(int dummyToAllowOverriding)
        {
            Send(MessageLeave);
        }

        public void CountStart(Vector pos, ROI.ROI roi, int count)
        {
            Send(String.Format("{0} 0 0 0", MessageCountStart));
        }

        public void CountStop()
        {
            Send(MessageCountStop);
        }

        public void CountComplete(Vector pos, ROI.ROI roi, DateTime time, int count)
        {
            Send(String.Format("{0} {1}", MessageCountSelect, roi.Name));
        }

        public void CountProgress(long dwellTime, ROI.ROI roi)
        {
            int progress = dwellTime < CountDetector.MIN_TIME ? 0
                : (int)((dwellTime - CountDetector.MIN_TIME) / (float)(CountDetector.DWELL - CountDetector.MIN_TIME) * 360);

            Send(String.Format("{0} {1} {2}", MessageCountProgress, progress, roi == null ? "x" : roi.Name));
        }

        public void CursorUpdate(Vector pos, int count, int edge)
        {
            Send(String.Format("{0} {1:0.00} {2:0.00} {3:0.00} {4} {5}", MessageCountUpdate, pos.x, pos.y, pos.z, count, edge));
        }

        public void GroupEnter(String name)
        {
            Send(String.Format("{0} {1}", MessageGroupEnter, name));
        }

        public void GroupLeave(String name)
        {
            Send(MessageGroupLeave);
        }

        public void Back()
        {
            Send(MessageBack);
        }

        public new void Send(String message)
        {
            if (base.Connected)
                base.Send(message);
        }
    }
}
