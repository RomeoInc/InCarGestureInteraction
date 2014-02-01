using System;
using Interfaces;
using Experiment;
using Leap.Gestures.Pointing;
using System.Threading;

namespace Leap.Gestures.Tap.Feedback
{
    public class TapTactile : NetworkInterface, IPointingObserver, ITapObserver
    {
        public static String MessageStop = "off";
        public static String MessageRampAmplitude = "ramp_a_exp";
        public static String MessageRampRoughness = "ramp_rough";
        public static String MessageRampFrequency = "ramp_f";
        public static String MessageConstant = "const";

        private bool startedDwell;
        private Metadata.FeedbackType type;

        public TapTactile(Metadata.FeedbackType type, String name)
            : base(name)
        {
            this.startedDwell = false;
            this.type = type;
        }

        #region IPointingTapObserver
        public void EnterWorkspace()
        {
            
        }

        public void LeaveWorkspace()
        {
            Send(MessageStop);
            startedDwell = false;
        }

        public void InitialPose()
        {
            
        }

        public void ExitPose()
        {
            Send(MessageStop);
            startedDwell = false;
        }

        public void StillPointing(Vector pos, DateTime when)
        {
            
        }
        #endregion

        #region ISelectionTapObserver
        public void EnterTarget(Vector pos, ROI.ROI roi)
        {
            //Send("on 175 0 0.1");
            //startedDwell = true;
        }

        public void LeaveTarget()
        {
            //Send("off");
            //startedDwell = false;
        }

        public void TapSelect(Vector pos, ROI.ROI roi, DateTime time)
        {

        }

        public void ProgressUpdate(double progress)
        {
            if (progress >= 0.3 && !startedDwell) {
                startedDwell = true;
                Send(String.Format("on 175 0 {0}", progress));
            } else if (progress < 0.5 && startedDwell) {
                startedDwell = false;
                Send("off");
            }
        }
        #endregion
    }
}
