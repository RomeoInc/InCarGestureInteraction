using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap.ROI;
using System.Drawing;
using Leap.Gestures.Pointing;

namespace Leap.Gestures.Tap
{
    class TapDetector : GestureDetector
    {
        #region Constants
        /// <summary>
        /// Minimum delta required for a tap gesture to be registered.
        /// </summary>
        public const float TAP_DELTA = -20.0f;

        /// <summary>
        /// Cooldown period between selections.
        /// </summary>
        public const long SELECTION_COOLDOWN = 400;

        /// <summary>
        /// Number of frames to buffer in average finger height calculations.
        /// </summary>
        private const int BUFFER_LENGTH = 30;

        /// <summary>
        /// Minimum finger angle to accept.
        /// </summary>
        private const int MIN_FINGER_ANGLE = -70;

        /// <summary>
        /// Maximum finger angle to accept.
        /// </summary>
        private const int MAX_FINGER_ANGLE = 50;
        #endregion

        #region Internal state
        public enum PointingState { Idle, Tracking, Pointing };
        private PointingState state;

        public enum SelectionState { Idle, Pointing };
        private SelectionState selectionState;

        #region Tap detection
        /// <summary>
        /// The average height of the finger from the entire buffer.
        /// </summary>
        private float averageHeight;

        /// <summary>
        /// Count of processed frames from the leap motion.
        /// </summary>
        private long frameCount;

        /// <summary>
        /// Total height of finger for the entire buffer.
        /// </summary>
        private float totalHeight;

        /// <summary>
        /// Buffer of finger heights.
        /// </summary>
        private float[] heightBuffer;

        /// <summary>
        /// Index of the next space in the buffer to be used.
        /// </summary>
        private int heightBufferIndex;
        #endregion

        /// <summary>
        /// Time at which the last selection occurred.
        /// </summary>
        private DateTime lastSelectedAt;

        /// <summary>
        /// The ROI where the cursor is currently dwelling. If the cursor leaves
        /// this ROI or moves to a new ROI while dwelling, the dwell period resets.
        /// </summary>
        private ROI.ROI targetROI;
        #endregion

        #region Observers
        private List<IPointingObserver> pointingObservers;
        private List<ITapObserver> selectionObservers;
        #endregion

        public TapDetector(LeapInterface leap, GestureSpace space)
        {
            // Initialise state machines
            state = PointingState.Idle;
            selectionState = SelectionState.Idle;

            averageHeight = 0;
            frameCount = 0;
            totalHeight = 0;
            heightBuffer = new float[BUFFER_LENGTH];
            heightBufferIndex = 0;

            // Initialise ROIs
            roiSets = new List<ROISet>();

            // Initialise observers
            pointingObservers = new List<IPointingObserver>();
            selectionObservers = new List<ITapObserver>();

            // Register gesture workspace
            this.RegisterWorkspace(space);

            // Register this gesture detector for frame updates
            leap.RegisterFrameListener(this);

            GestureDetector.DEBUG = false;
        }

        #region IPointingObserver
        public void RegisterPointingListener(IPointingObserver listener)
        {
            pointingObservers.Add(listener);
        }

        public void UnregisterPointingListener(IPointingObserver listener)
        {
            pointingObservers.Remove(listener);
        }
        #endregion

        #region ISelectionObserver
        public void RegisterSelectionListener(ITapObserver listener)
        {
            selectionObservers.Add(listener);
        }

        public void UnregisterSelectionListener(ITapObserver listener)
        {
            selectionObservers.Remove(listener);
        }
        #endregion

        #region Leap
        /// <summary>
        /// Process a frame update from the Leap interface.
        /// </summary>
        /// 
        /// <param name="frame">Leap frame to process.</param>
        public override void FrameUpdate(Frame frame)
        {
            switch (state)
            {
                case PointingState.Idle:
                    ProcessFrameIdle(frame);
                    break;
                case PointingState.Pointing:
                    ProcessFramePointing(frame);
                    break;
                case PointingState.Tracking:
                    ProcessFrameTracking(frame);
                    break;
                default:
                    break;
            }
        }

        #region Idle
        /// <summary>
        /// Process a frame whilst in the Idle state. In this state, the
        /// hand is not actively being tracked as it is not visible. This
        /// state can be left by the hand entering tracking range, or by
        /// the hand entering tracking range in the initial pose.
        /// </summary>
        /// 
        /// <param name="frame">Leap Frame to process.</param>
        private void ProcessFrameIdle(Frame frame)
        {
            List<Hand> hands = DetectedHands(frame.Hands);
            FingerList fingers = frame.Fingers;
            List<Finger> extendedFingers = ExtendedFingers(fingers, true);

            if (hands.Count == 0 && extendedFingers.Count == 0)
            {
                // No hands or fingers visible; stay in this state.
                return;
            }
            else if (hands.Count > 0)
            {
                // No fingers - we've only seen the hand
                EnterWorkspace();
            }
            else if (extendedFingers.Count > 0)
            {
                // Check if pointing finger is in the correct pointing pose.
                bool inPose = InPose(extendedFingers);

                EnterWorkspace();

                if (inPose)
                {
                    InitialPose();
                }
            }
        }
        #endregion

        #region Tracking
        /// <summary>
        /// Process a frame whilst in the Tracking state. In this state, we're
        /// able to track the hand but the user hasn't entered the initial
        /// pointing pose. This state can be left by either leaving the workspace
        /// or performing the initial pointing pose.
        /// </summary>
        /// 
        /// <param name="frame">Leap Frame to process.</param>
        private void ProcessFrameTracking(Frame frame)
        {
            List<Hand> hands = DetectedHands(frame.Hands);
            FingerList fingers = frame.Fingers;
            List<Finger> extendedFingers = ExtendedFingers(fingers, true);

            if (hands.Count == 0 && extendedFingers.Count == 0)
            {
                // Hands no longer visible - leave this state
                LeaveWorkspace();

                return;
            }
            else if (extendedFingers.Count == 0)
            {
                // No fingers visible - stay in this state
                return;
            }

            // At least one finger is visible - check if it's in the right pose and within the gesturing space
            bool inPose = InPose(extendedFingers);

            if (inPose)
            {
                InitialPose();
            }
        }
        #endregion

        #region Pointing
        /// <summary>
        /// Process a frame whilst in the Pointing state. In this state, the
        /// finger is controlling a pointer. During this state, a "tap" gesture
        /// may occur. This is dispatched to all registered selection receivers.
        /// This state may be left by the hand leaving the workspace, or if the
        /// hand stops performing the pointing pose.
        /// </summary>
        /// 
        /// <param name="frame">Leap Frame to process.</param>
        private void ProcessFramePointing(Frame frame)
        {
            List<Hand> hands = DetectedHands(frame.Hands);
            FingerList fingers = frame.Fingers;
            List<Finger> extendedFingers = ExtendedFingers(fingers, true);

            if (hands.Count == 0 && extendedFingers.Count == 0)
            {
                // Hands no longer visible - leave this state
                // LeaveWorkspace();

                /*
                 * Previously called LeaveWorkspace, but that was causing
                 * problems in my experiment code. Because the full hand
                 * wasn't necessarily visible when the finger disappeared,
                 * it was saying that the hand was no longer in the Leap
                 * field of view, rather than that there were no visible
                 * extended fingers anymore.
                 */
                ExitPose();

                return;
            }
            else if (extendedFingers.Count == 0)
            {
                // No fingers visible - we can't track any more
                ExitPose();

                return;
            }

            // Fingers are still visible - check if they're still in the pointing pose and within the gesturing space
            bool inPose = InPose(extendedFingers);

            if (!inPose)
            {
                ExitPose();

                return;
            }

            #region Update finger height
            float y = extendedFingers[0].TipPosition.y;

            ++frameCount;
            heightBuffer[heightBufferIndex] = y;
            heightBufferIndex = (heightBufferIndex + 1) % BUFFER_LENGTH;

            totalHeight = totalHeight + y - (frameCount > BUFFER_LENGTH ? heightBuffer[heightBufferIndex] : 0);

            float delta = y - averageHeight;

            if (frameCount > BUFFER_LENGTH)
            {
                averageHeight = totalHeight / BUFFER_LENGTH;

                //Console.WriteLine("{0:0.00} {1:0.00} {2:0.00}", averageHeight, y, delta);
            }
            #endregion

            // Check average position of fingers
            Vector avgPos = AveragePos(extendedFingers);

            #region Check if targeting
            ROI.ROI roi = FindROI(new Point((int)avgPos.x, (int)avgPos.z));

            bool onTarget = roi != null;

            if (onTarget)
            {
                #region Check tap gesture
                bool tapped = false;

                if (delta <= TAP_DELTA && frameCount > 50)
                {
                    tapped = true;
                }
                #endregion

                if (selectionState == SelectionState.Idle)
                {
                    bool goodToGo = false;

                    // See if the cooldown has expired before moving on
                    if (lastSelectedAt != DateTime.MinValue)
                    {
                        double timeSinceLast = DateTime.Now.Subtract(lastSelectedAt).TotalMilliseconds;

                        if (timeSinceLast >= SELECTION_COOLDOWN)
                            goodToGo = true;
                    }
                    else
                        goodToGo = true;

                    if (goodToGo)
                    {
                        // Enter a target
                        EnterTarget(avgPos, roi);

                        targetROI = roi;
                    }
                }
                else if (selectionState == SelectionState.Pointing)
                {
                    if (targetROI != roi)
                    {
                        // Leave target - next frame update will notice if we enter a new one
                        LeaveTarget();
                        return;
                    }

                    if (tapped)
                    {
                        // We've tapped above a target to select it
                        TapSelect(avgPos, roi);

                        lastSelectedAt = DateTime.Now;

                        ProgressUpdate(0);
                    } else {
                        ProgressUpdate((delta) / TAP_DELTA);
                    }
                }
            } else {
                // No longer on a target, although we previously were
                if (selectionState != SelectionState.Idle)
                {
                    LeaveTarget();
                }
            }
            #endregion
            
            DateTime now = DateTime.Now;

            // Update the position of the cursor
            foreach (IPointingObserver listener in pointingObservers)
            {
                if (workspace != null)
                    listener.StillPointing(workspace.Normalise(avgPos), now);
                else
                    listener.StillPointing(avgPos, now);
            }
        }
        #endregion

        #region Pose Check
        /// <summary>
        /// Determines if the hand is in the pointing pose. The hand
        /// is in the pointing pose when the finger is roughly flat
        /// (in the range of -50 degrees to 50 degrees), when the
        /// length of the finger exceeds 30 cm, and when the finger
        /// tip position is within the gesture workspace.
        /// </summary>
        private bool InPose(List<Finger> fingers)
        {
            bool inPose = false;

            Vector avgPos = AveragePos(fingers);

            foreach (Finger finger in fingers)
            {
                if (!finger.IsValid)
                    continue;

                double pitchDeg = finger.Direction.Pitch * (180 / Math.PI);

                inPose =
                    MIN_FINGER_ANGLE <= pitchDeg && pitchDeg <= MAX_FINGER_ANGLE
                    && finger.Length >= MIN_FINGER_LENGTH;
            }

            if (workspace != null)
                return inPose && workspace.Contains(avgPos);
            else
                return inPose;
        }
        #endregion
        #endregion

        #region Events
        /// <summary>
        /// Hand enters the gesture workspace and can now be tracked.
        /// </summary>
        private void EnterWorkspace()
        {
            // If we aren't Idle, we're already in the workspace.
            if (state != PointingState.Idle)
                return;

            state = PointingState.Tracking;
            selectionState = SelectionState.Idle;

            foreach (IPointingObserver listener in pointingObservers)
            {
                listener.EnterWorkspace();
            }

            Log("Entered Workspace");
        }

        /// <summary>
        /// Hand leaves the gesture workspace and can no longer be tracked.
        /// </summary>
        private void LeaveWorkspace()
        {
            // If we're already Idle, we can't have left the workspace.
            if (state == PointingState.Idle)
                return;

            if (selectionState == SelectionState.Pointing)
                LeaveTarget();

            state = PointingState.Idle;
            selectionState = SelectionState.Idle;

            foreach (IPointingObserver listener in pointingObservers)
            {
                listener.LeaveWorkspace();
            }

            Log("Left Workspace");
        }

        /// <summary>
        /// Hand enters the initial pointing pose, i.e. index finger extended.
        /// </summary>
        private void InitialPose()
        {
            // If we're already Pointing, we've already registered the initial pose.
            if (state == PointingState.Pointing)
                return;

            // If we came here directly from idle, first trigger entering the workspace
            if (state == PointingState.Idle)
            {
                EnterWorkspace();
            }

            state = PointingState.Pointing;
            selectionState = SelectionState.Idle;

            foreach (IPointingObserver listener in pointingObservers)
            {
                listener.InitialPose();
            }

            Log("Initial Pose");
        }

        /// <summary>
        /// Hand leaves the pointing pose, i.e. all fingers are retracted.
        /// </summary>
        private void ExitPose()
        {
            // If we're not already Pointing, we can't have left the pose.
            if (state != PointingState.Pointing)
                return;

            if (selectionState == SelectionState.Pointing)
                LeaveTarget();

            state = PointingState.Tracking;
            selectionState = SelectionState.Idle;

            foreach (IPointingObserver listener in pointingObservers)
            {
                listener.ExitPose();
            }

            Log("Exit Pose");
        }
        #endregion

        #region Tapping
        private void EnterTarget(Vector pos, ROI.ROI roi)
        {
            selectionState = SelectionState.Pointing;
            targetROI = roi;

            Log("EnterTarget");

            foreach (ITapObserver observer in selectionObservers)
            {
                if (workspace != null)
                    observer.EnterTarget(workspace.Normalise(pos), roi);
                else
                    observer.EnterTarget(pos, roi);
            }
        }

        private void LeaveTarget()
        {
            selectionState = SelectionState.Idle;
            targetROI = null;

            Log("LeaveTarget");

            foreach (ITapObserver observer in selectionObservers)
                observer.LeaveTarget();

            // Reset cursor when it leaves a target
            ProgressUpdate(0);
        }

        private void TapSelect(Vector pos, ROI.ROI roi)
        {
            selectionState = SelectionState.Pointing;
            DateTime now = DateTime.Now;

            Log("Tap select: " + roi.Name);

            foreach (ITapObserver observer in selectionObservers)
            {
                if (workspace != null)
                    observer.TapSelect(workspace.Normalise(pos), roi, now);
                else
                    observer.TapSelect(pos, roi, now);

                // Trigger a ROI set activation
                if (roi.ToActivate != null)
                    ActivateROISet(roi.ToActivate);
            }
        }

        private void ProgressUpdate(double progress)
        {
            foreach (ITapObserver observer in selectionObservers)
                observer.ProgressUpdate(progress);
        }
        #endregion
    }
}
