using System;
using Interfaces;
using Experiment;

namespace Leap.Gestures.Count.Feedback
{
    class CountTactile : NetworkInterface, ICountObserver
    {
        public static String MessageStop = "off";
        public static String MessageRampAmplitude = "ramp_a_exp";
        public static String MessageRampRoughness = "ramp_rough";
        public static String MessageRampFrequency = "ramp_f";
        public static String MessageConstant = "const";
        public static String MessagePulse = "pulse";

        private Metadata.FeedbackType type;
        private bool started;

        public CountTactile(Metadata.FeedbackType type, String name)
            : base(name)
        {
            this.type = type;
            this.started = false;
        }

        public void EnterWorkspace(int hands, int fingers)
        {
            
        }

        public void LeaveWorkspace(int dummyToAllowOverriding)
        {
            Send(MessageStop);
        }

        public void CountStart(Vector pos, ROI.ROI roi, int count)
        {
            
        }

        public void CountStop()
        {
            started = false;
            Send(MessageStop);
        }

        public void CountComplete(Vector pos, ROI.ROI roi, DateTime time, int count)
        {

        }

        public void CountProgress(long dwellTime, ROI.ROI roi)
        {
            if (dwellTime > CountDetector.MIN_TIME && !started)
            {
                started = true;

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
                    case Metadata.FeedbackType.None: // TEST CODE
                        //Send(MessageRampFrequency);
                        break;
                }
            }
        }

        public void CursorUpdate(Vector pos, int count, int edge)
        {
            
        }

        public void GroupEnter(String name)
        {
            
        }

        public void GroupLeave(String name)
        {
            
        }

        public void Back()
        {
            Send(MessagePulse);
        }
    }
}
