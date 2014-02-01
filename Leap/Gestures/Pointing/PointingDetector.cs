using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap.ROI;
using System.Drawing;

namespace Leap.Gestures.Pointing
{
    class PointingDetector : GestureDetector
    {
        #region Constants
        /// <summary>
        /// Minimum time to dwell before showing feedback
        /// </summary>
        public const int MIN_TIME = 250;

        public const long DWELL = 1250; // Dwell time in milliseconds
        public const long DWELL_COOLDOWN = 400; // Cooldown period between dwells
        private const int MIN_FINGER_ANGLE = -50;
        private const int MAX_FINGER_ANGLE = 50;
        #endregion

        #region Internal state
        public enum PointingState { Idle, Tracking, Pointing };
        private PointingState state;
        private int activeHandId;
        private int activeFingerId;

        public enum SelectionState { Idle, Pointing, Complete };
        private SelectionState selectionState;
        
        /// <summary>
        /// Time at which the dwell started.
        /// </summary>
        private DateTime dwellStart;

        /// <summary>
        /// Time at which the last selection occurred.
        /// </summary>
        private DateTime dwellSelectedAt;

        /// <summary>
        /// The ROI where the cursor is currently dwelling. If the cursor leaves
        /// this ROI or moves to a new ROI while dwelling, the dwell period resets.
        /// </summary>
        private ROI.ROI dwellROI;
        #endregion

        #region Observers
        private List<IPointingObserver> pointingObservers;
        private List<ISelectionObserver> selectionObservers;
        #endregion

        public PointingDetector(LeapInterface leap, GestureSpace space)
        {
            // Initialise state machines
            state = PointingState.Idle;
            selectionState = SelectionState.Idle;
            dwellStart = DateTime.Now;
            activeHandId = -1;
            activeFingerId = -1;

            // Initialise ROIs
            roiSets = new List<ROISet>();

            // Initialise observers
            pointingObservers = new List<IPointingObserver>();
            selectionObservers = new List<ISelectionObserver>();

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
        public void RegisterSelectionListener(ISelectionObserver listener)
        {
            selectionObservers.Add(listener);
        }

        public void UnregisterSelectionListener(ISelectionObserver listener)
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
            #region Hand check
            List<Hand> hands = DetectedHands(frame.Hands);

            if (hands.Count == 0)
                return;
            #endregion

            // Get ID of hand and check which of its fingers are extended
            int handId = hands[0].Id;
            List<Finger> extendedFingers = ExtendedFingers(hands[0].Fingers, true);

            if (extendedFingers.Count == 0)
            {
                // No hands or fingers visible; stay in this state.
                return;
            }
            else if (hands.Count > 0)
            {
                // No fingers - we've only seen the hand
                EnterWorkspace(handId);
            }
            else if (extendedFingers.Count > 0)
            {
                #region Finger check
                Finger finger = extendedFingers[0];
                List<Finger> fingers = new List<Finger>(new Finger[] {finger});

                // Check if pointing finger is in the correct pointing pose.
                bool inPose = InPose(fingers);
                #endregion

                EnterWorkspace(handId);

                if (inPose)
                {
                    InitialPose(handId, finger.Id);
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
            #region Hand check
            List<Hand> hands = DetectedHands(frame.Hands);

            if (hands.Count == 0)
            {
                LeaveWorkspace();
                return;
            }

            Hand activeHand = null;
            foreach (Hand hand in hands)
            {
                if (hand.Id == activeHandId)
                {
                    activeHand = hand;
                    break;
                }
            }

            if (activeHand == null)
            {
                // Lost sight of the active hand
                LeaveWorkspace();
                return;
            }
            #endregion

            List<Finger> extendedFingers = ExtendedFingers(activeHand.Fingers, true);

            if (extendedFingers.Count == 0)
            {
                // No fingers visible - stay in this state
                return;
            }

            #region Finger check
            Finger finger = extendedFingers[0];
            List<Finger> fingers = new List<Finger>(new Finger[] {finger});

            // Check if pointing finger is in the correct pointing pose.
            bool inPose = InPose(fingers);
            #endregion

            if (inPose)
            {
                InitialPose(activeHand.Id, finger.Id);
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
            #region Hand check
            List<Hand> hands = DetectedHands(frame.Hands);

            if (hands.Count == 0)
            {
                LeaveWorkspace();
                return;
            }

            Hand activeHand = null;
            foreach (Hand hand in hands)
            {
                if (hand.Id == activeHandId)
                {
                    activeHand = hand;
                    break;
                }
            }

            if (activeHand == null)
            {
                // Lost sight of the active hand
                LeaveWorkspace();
                return;
            }
            #endregion

            List<Finger> extendedFingers = ExtendedFingers(activeHand.Fingers, true);

            if (extendedFingers.Count == 0)
            {
                // No fingers visible - we can't track any more
                ExitPose();

                return;
            }

            #region Finger check
            Finger activeFinger = null;
            foreach (Finger finger in extendedFingers)
            {
                if (finger.Id == activeFingerId)
                {
                    activeFinger = finger;
                    break;
                }
            }

            if (activeFinger == null)
            {
                // Lost sight of pointing finger
                ExitPose();
                return;
            }

            List<Finger> fingers = new List<Finger>(new Finger[] {activeFinger});

            // Check if pointing finger is in the correct pointing pose.
            bool inPose = InPose(fingers);
            #endregion

            if (!inPose)
            {
                ExitPose();

                return;
            }

            // Check average position of fingers
            Vector avgPos = activeFinger.TipPosition;//AveragePos(extendedFingers);

            // Check if we're dwelling
            ROI.ROI roi = FindROI(new Point((int)avgPos.x, (int)avgPos.z));

            bool dwelling = roi != null;

            if (dwelling)
            {
                if (selectionState == SelectionState.Idle)
                {
                    bool goodToGo = false;

                    // See if the cooldown has expired before moving on
                    if (dwellSelectedAt != DateTime.MinValue)
                    {
                        double timeSinceLast = DateTime.Now.Subtract(dwellSelectedAt).TotalMilliseconds;

                        if (timeSinceLast >= DWELL_COOLDOWN)
                            goodToGo = true;
                    }
                    else
                        goodToGo = true;

                    if (goodToGo)
                    {
                        // Start a dwell
                        StartDwell(avgPos, roi);

                        dwellROI = roi;
                    }
                }
                else if (selectionState == SelectionState.Pointing)
                {
                    if (dwellROI != roi)
                    {
                        StopDwell();
                        return;
                    }

                    // Check if we've exceeded the dwell time
                    double delta = DateTime.Now.Subtract(dwellStart).TotalMilliseconds;

                    if (delta >= DWELL)
                    {
                        // We've exceeded the dwell time - gesture complete!
                        DwellSelect(avgPos, roi);

                        dwellSelectedAt = DateTime.Now;
                    }

                    DwellProgressUpdate((long)delta);
                }
                else if (selectionState == SelectionState.Complete)
                {
                    // If a dwell has finished but the pointer moves to a new ROI, start over
                    if (dwellROI != roi)
                    {
                        StopDwell();
                    }
                }
            }
            else
            {
                if (selectionState != SelectionState.Idle)
                {
                    StopDwell();
                }
            }

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
        private void EnterWorkspace(int handId)
        {
            // If we aren't Idle, we're already in the workspace.
            if (state != PointingState.Idle)
                return;

            state = PointingState.Tracking;
            selectionState = SelectionState.Idle;
            activeHandId = handId;

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
                StopDwell();

            state = PointingState.Idle;
            selectionState = SelectionState.Idle;
            activeHandId = -1;

            foreach (IPointingObserver listener in pointingObservers)
            {
                listener.LeaveWorkspace();
            }

            Log("Left Workspace");
        }

        /// <summary>
        /// Hand enters the initial pointing pose, i.e. index finger extended.
        /// </summary>
        private void InitialPose(int handId, int fingerId)
        {
            // If we're already Pointing, we've already registered the initial pose.
            if (state == PointingState.Pointing)
                return;

            // If we came here directly from idle, first trigger entering the workspace
            if (state == PointingState.Idle)
            {
                EnterWorkspace(handId);
            }

            state = PointingState.Pointing;
            selectionState = SelectionState.Idle;
            activeFingerId = fingerId;

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
                StopDwell();

            state = PointingState.Tracking;
            selectionState = SelectionState.Idle;
            activeFingerId = -1;

            foreach (IPointingObserver listener in pointingObservers)
            {
                listener.ExitPose();
            }

            Log("Exit Pose");
        }
        #endregion

        #region Dwelling
        /// <summary>
        /// A dwell selection has started.
        /// </summary>
        private void StartDwell(Vector pos, ROI.ROI roi)
        {
            dwellStart = DateTime.Now;
            selectionState = SelectionState.Pointing;
            dwellROI = roi;

            Log(String.Format("Dwell started: [{0}] {1}:{2}.{3}.{4}", roi.Name, dwellStart.Hour, dwellStart.Minute, dwellStart.Second, dwellStart.Millisecond));

            foreach (ISelectionObserver listener in selectionObservers)
            {
                if (workspace != null)
                    listener.StartDwell(workspace.Normalise(pos), roi);
                else
                    listener.StartDwell(pos, roi);
            }
        }

        /// <summary>
        /// A dwell selection has stopped before completion.
        /// </summary>
        private void StopDwell()
        {
            selectionState = SelectionState.Idle;
            dwellROI = null;

            DateTime now = DateTime.Now;

            Log(String.Format("Dwell stopped: {0}:{1}.{2}.{3} ({4} ms)", now.Hour, now.Minute, now.Second, now.Millisecond, now.Subtract(dwellStart).TotalMilliseconds));

            foreach (ISelectionObserver listener in selectionObservers)
            {
                listener.StopDwell();
            }

            // Reset cursor when it leaves a target
            DwellProgressUpdate(0);
        }

        /// <summary>
        /// A dwell selection has completed.
        /// </summary>
        private void DwellSelect(Vector pos, ROI.ROI roi)
        {
            selectionState = SelectionState.Complete;

            DateTime now = DateTime.Now;

            Log(String.Format("Selection: [{0}] {1}:{2}.{3}.{4} ({5} ms)", roi.Name, now.Hour, now.Minute, now.Second, now.Millisecond, now.Subtract(dwellStart).TotalMilliseconds));

            foreach (ISelectionObserver listener in selectionObservers)
            {
                if (workspace != null)
                    listener.DwellSelect(workspace.Normalise(pos), roi, now);
                else
                    listener.DwellSelect(pos, roi, now);

                // Trigger a ROI set activation
                if (roi.ToActivate != null)
                {
                    ActivateROISet(roi.ToActivate);
                }
            }
        }

        private void DwellProgressUpdate(long dwellTime)
        {
            foreach (ISelectionObserver listener in selectionObservers)
            {
                listener.ProgressUpdate(dwellTime);
            }
        }
        #endregion
    }
}
