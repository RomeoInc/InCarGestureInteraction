using System;
using System.Collections.Generic;
using Leap;

namespace Leap
{
    public class LeapListener : Listener
    {
        public const bool DEBUG = true;
        public const bool DEBUG_VERBOSE = false;

        private List<IFrameListener> frameListeners;

        public LeapListener()
        {
            frameListeners = new List<IFrameListener>();
        }

        #region Frames
        public override void OnFrame(Controller controller)
        {
            Frame frame = controller.Frame();

            foreach (IFrameListener frameListener in frameListeners)
            {
                frameListener.OnFrame(frame);
            }
        }

        public void RegisterFrameListener(IFrameListener listener)
        {
            frameListeners.Add(listener);
        }

        public void UnregisterFrameListener(IFrameListener listener)
        {
            frameListeners.Remove(listener);
        }
        #endregion

        #region Connectivity
        public override void OnInit(Controller controller)
        {
            Log("Initialised");
        }

        public override void OnConnect(Controller controller)
        {
            Log("Leap Connected");

            // Enable gestures as appropriate
            controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
            controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            Log("Leap Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            Log("Leap Exiting");
        }
        #endregion

        #region Utility
        private void Log(String message)
        {
            lock (this)
            {
                if (DEBUG)
                {
                    Console.WriteLine("LeapListener: {0}", message);
                }
            }
        }

        /// <summary>
        /// Log diagnostic information about the given Frame object.
        /// </summary>
        private void LogFrame(Frame frame)
        {
            Log(String.Format("Frame {0}, Hands: {1}, Fingers: {2}, Tools: {3}, Gestures: {4}",
                frame.Id, frame.Hands.Count, frame.Fingers.Count, frame.Tools.Count, frame.Gestures().Count));
        }
        #endregion
    }
}