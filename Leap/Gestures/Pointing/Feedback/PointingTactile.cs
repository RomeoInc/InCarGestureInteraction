using System;
using Interfaces;
using Leap.Gestures.Pointing;
using Experiment;

namespace Leap.Gestures.Pointing.Feedback
{
    public class PointingTactile : NetworkInterface, IPointingObserver, ISelectionObserver
    {
        public static String MessageStop = "off";
        public static String MessageRampAmplitude = "ramp_a_exp";
        public static String MessageRampRoughness = "ramp_rough";
        public static String MessageConstant = "const";

        private bool startedDwell;
        private Metadata.FeedbackType type;

        public PointingTactile(Metadata.FeedbackType type, String name)
            : base(name)
        {
            this.startedDwell = false;
            this.type = type;
        }

        #region IDwellPointingObserver
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

        #region IDwellSelectionObserver
        public void StartDwell(Vector pos, ROI.ROI roi)
        {
            
        }

        public void StopDwell()
        {
            startedDwell = false;
            Send(MessageStop);
        }

        public void DwellSelect(Vector pos, ROI.ROI roi, DateTime time)
        {
            startedDwell = false;
        }

        public void ProgressUpdate(long dwellTime)
        {
            if (dwellTime > PointingDetector.MIN_TIME && !startedDwell)
            {
                startedDwell = true;

                switch (type)
                {
                    case Metadata.FeedbackType.Constant:
                        Send(MessageConstant);
                        break;
                    case Metadata.FeedbackType.Amplitude:
                        Send(MessageRampAmplitude);
                        break;
                    case Metadata.FeedbackType.Roughness:
                        Send(MessageRampRoughness);
                        break;
                    case Metadata.FeedbackType.None:
                        break;
                }
            }
        }
        #endregion
    }
}
