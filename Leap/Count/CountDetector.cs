using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Interfaces;


namespace Leap.Gestures.Count
{
    class CountDetector : GestureDetector, INetworkObserver
    {
        #region Constants
        /// <summary>
        /// Minimum time to dwell before showing feedback.
        /// </summary>
        public const long MIN_TIME = 250;

        /// <summary>
        /// Dwell time.
        /// </summary>
        public const long DWELL = 1250;

        /// <summary>
        /// Cooldown period between dwell selections.
        /// </summary>
        public const long DWELL_COOLDOWN = 400;

        /// <summary>
        /// How many frames to discard before starting to track
        /// a finger count gesture.
        /// </summary>
        public const int READY_FRAMES = 10;

        /// <summary>
        /// Lowest error in finger count which is acceptable. This is
        /// an amount above the count value, e.g. if the count gesture
        /// is for two fingers, a gesture is considered ongoing so long
        /// as the average is less than (two + LOWER_THRESHOLD).
        /// </summary>
        public const double LOWER_THRESHOLD = 0.10;

        /// <summary>
        /// Opposite of the lower threshold. Acceptable average is
        /// more than (value - 1 + UPPER_THRESHOLD).
        /// </summary>
        public const double UPPER_THRESHOLD = 1.0 - LOWER_THRESHOLD;

        /// <summary>
        /// How many mm close to group edge before showing warning.
        /// </summary>
        public const int EDGE_THRESHOLD = 20;

        /// <summary>
        /// Minimum speed for a swipe gesture.
        /// </summary>
        public const int SWIPE_SPEED = 1300;

        /// <summary>
        /// Minimum speed for a swipe gesture.
        /// </summary>
        public const int POWER_SWIPE = 2100;

        /// <summary>
        /// Minimum x direction for the swipe left gesture.
        /// </summary>
        public const float SWIPE_LEFT_DIRECTION = -0.85f;

        /// <summary>
        /// Minimum x direction for the swipe right gesture.
        /// </summary>
        public const float SWIPE_RIGHT_DIRECTION = 0.85f;

        /// <summary>
        /// Minimum y direction for the swipe up gesture.
        /// </summary>
        public const float SWIPE_UP_DIRECTION = 0.85f;

        /// <summary>
        /// Minimum y direction for the swipe down gesture.
        /// </summary>
        public const float SWIPE_DOWN_DIRECTION = -0.85f;

        /// <summary>
        /// Minimum z direction for the zoom in gesture.
        /// </summary>
        public const float ZOOM_IN_DIRECTION = -0.6f;

        /// <summary>
        /// Minimum z direction for the zoom out gesture.
        /// </summary>
        public const float ZOOM_OUT_DIRECTION = 0.5f;

        /// <summary>
        /// Return -1 if the gestrue does not meet the criteria.
        /// </summary>
        public const int INVALID_GESTURE = -1;
        #endregion

        public enum CountState { Idle, Tracking, Ready, Counting };
        private CountState state;

        public enum CountSelectionState { Idle, InProgress, Complete };
        private CountSelectionState selectionState;

        public enum AcceptedGestures
        {
            GoBack,
            SwipeLeft,
            AnswerCall,
            SwipeRight,
            DriverClosed,
            PassengerClosed,
            BothClosed,
            SwipeUp,
            DriverOpen,
            PassengerOpen,
            BothOpen,
            SwipeDown,
            SwipeIn,
            SwipeOut,
            RotateClockwise,
            RotateAntiCLockwise,
            SelectOption
        }; 
        private AcceptedGestures gestureType;

        private List<ICountObserver> observers;
        private DateTime countStart;
        private DateTime countSelectedAt;
        private int countFrames;
        private int countTotal;
        private int activeHandId;
        private int discardedFrames;
        private int activeGroup;

        public CountDetector(LeapInterface leap, GestureSpace space)
        {
            // Initialise state machines
            state = CountState.Idle;
            selectionState = CountSelectionState.Idle;
            activeHandId = -1;
            discardedFrames = 0;

            // Initialise groups and ROIs
            activeGroup = 0;

            // Initialise list of observers
            observers = new List<ICountObserver>();

            // Initialise selection metadata
            countStart = DateTime.Now;

            // Register gesture workspace
            this.RegisterWorkspace(space);

            // Register this gesture detector for frame updates
            leap.RegisterFrameListener(this);

            GestureDetector.DEBUG = false;
        }

        /*#region INetworkObserver
        public void MessageReceived(String message) {
            if (message.Equals("back")) {
                Back();
            }
        }
        #endregion */

        /// <summary>
        /// Registers a gesture workspace for this state machine. The
        /// hand is only considered to be in the correct pose if it
        /// is within this area. Workspace will be adjusted since,
        /// unlike the default workspace, we want interaction to take
        /// place a bit further back from the phone. Otherwise the user
        /// would have to reach too far and this also poses finger
        /// tracking problems.
        /// </summary>
        public new void RegisterWorkspace(GestureSpace workspace)
        {
            Rectangle space = workspace.GetRectangle();

            int bottom = GestureSpace.Height / 2;
            int top = bottom - GestureSpace.Height;

            GestureSpace adjustedSpace = new GestureSpace(space.Left, top, space.Right, bottom);

            base.RegisterWorkspace(adjustedSpace);
        }

        #region Leap
        /// <summary>
        /// Process a frame update from the Leap interface.
        /// </summary>
        public override void FrameUpdate(Frame frame)
        {
            switch (state)
            {
                case CountState.Idle:
                    ProcessFrameIdle(frame);
                    break;
                case CountState.Tracking:
                    ProcessFrameTracking(frame);
                    break;
                case CountState.Ready:
                    ProcessFrameReady(frame);
                    break;
                case CountState.Counting:
                    ProcessFrameCounting(frame);
                    break;
            }

            // Check for swipe gestures
            if (SwipeLeft(frame) != INVALID_GESTURE && !base.activeSet.Equals("Main"))
            {
                foreach (IGestureObserver observer in observers)
                {
                    observer.gestureComplete(SwipeLeft(frame));
                }
            }

            if (SwipeRight(frame) != INVALID_GESTURE && !base.activeSet.Equals("Main"))
            {
                foreach (IGestureObserver observer in observers)
                {
                    observer.gestureComplete(SwipeRight(frame));
                }
            }

            if (SwipeUp(frame) != INVALID_GESTURE && !base.activeSet.Equals("Main"))
            {
                foreach (IGestureObserver observer in observers)
                {
                    observer.gestureComplete(SwipeUp(frame));
                }
            }

        }

        /*private void Back() {
            foreach (ICountObserver observer in observers)
                observer.Back();

            ROI.ROI roi = base.FindROI("Back");

            // Trigger a ROI set activation
            if (roi != null && roi.ToActivate != null)
            {
                ActivateROISet(roi.ToActivate);
            }
        }*/

        /// <summary>
        /// Process a frame update while in the Idle state. In this
        /// state, the hand is not visible.
        /// </summary>
        /// <param name="frame"></param>
        private void ProcessFrameIdle(Frame frame)
        {
            #region Hand check
            List<Hand> hands = DetectedHands(frame.Hands);

            if (hands.Count == 0)
                return;
            #endregion

            // Get ID of hand and check which of its fingers are extended
            int handId = hands[0].Id;
            List<Finger> fingers = ExtendedFingers(hands[0].Fingers, false);

            if (fingers.Count > 0)
            {
                // Tracking hands and fingers
                EnterWorkspace(handId, fingers.Count);

                PoseStart();
            }
            else
            {
                // Only hand(s) visible
                EnterWorkspace(handId, 0);
            }
        }

        /// <summary>
        /// Process a frame while in the Tracking state. In this
        /// state, the hand is visible but no fingers are.
        /// </summary>
        /// <param name="frame"></param>
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

            #region Groups
            // If a group switch has taken place, abort selection
            if (ProcessGroups(activeHand))
                return;
            #endregion

            List<Finger> fingers = ExtendedFingers(activeHand.Fingers, false);

            if (fingers.Count > 0)
            {
                // Fingers visible - start counting
                PoseStart();
            }
        }

        /// <summary>
        /// Process a frame update while in the Ready state. In this
        /// state, the gesture detector is waiting for a finger count
        /// to stabilise before starting gesture detection.
        /// </summary>
        private void ProcessFrameReady(Frame frame)
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

            if (ExtendedFingers(activeHand.Fingers, false).Count == 0)
                PoseEnd();
            #endregion

            #region Groups
            // If a group switch has taken place, abort selection
            if (ProcessGroups(activeHand))
                return;
            #endregion

            discardedFrames++;

            if (discardedFrames == READY_FRAMES)
            {
                Log("Ready to count");

                state = CountState.Counting;
            }
        }

        /// <summary>
        /// Process a frame update while in the Counting state. In
        /// this state, the hand is visible and fingers are being tracked.
        /// </summary>
        /// <param name="frame"></param>
        private void ProcessFrameCounting(Frame frame)
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

            List<Finger> fingers = ExtendedFingers(activeHand.Fingers, false);

            if (fingers.Count == 0)
            {
                PoseEnd();
            }
            else
            {
                #region Groups
                if (ProcessGroups(activeHand))
                    return;
                #endregion

                switch (selectionState)
                {
                    case CountSelectionState.Idle:
                        #region Idle
                        bool goodToGo = false;

                        if (countSelectedAt != DateTime.MinValue)
                        {
                            double timeSinceLast = DateTime.Now.Subtract(countSelectedAt).TotalMilliseconds;

                            if (timeSinceLast > DWELL_COOLDOWN)
                                goodToGo = true;
                        }
                        else
                        {
                            goodToGo = true;
                        }

                        if (goodToGo)
                        {
                            // Start counting!
                            ROI.ROI roi = FindROIWithinGroup(fingers.Count);

                            if (roi != null) {
                                CountStart(roi, fingers.Count);
                            }
                        }
                        #endregion
                        break;
                    case CountSelectionState.InProgress:
                        #region InProgress
                        #region Finger counting
                        int frameCount = fingers.Count;
                        countFrames++;
                        countTotal += frameCount;
                        double averageCount = countTotal / (double)countFrames;
                        double mod = averageCount % 1.0;
                        int count = (int)Math.Round(averageCount);
                        #endregion

                        if (mod > LOWER_THRESHOLD && mod < UPPER_THRESHOLD)
                        {
                            // There's either too much noise or the user has changed
                            // their selection goal. Either way, stop counting.
                            CountStop();
                            break;
                        }

                        ROI.ROI targetedROI = FindROIWithinGroup(count);

                        if (targetedROI != null)
                        {
                            // Check if we've exceeded the dwell time
                            double delta = DateTime.Now.Subtract(countStart).TotalMilliseconds;

                            if (delta >= DWELL)
                            {
                                CountSelect(targetedROI, DateTime.Now, count);
                            }
                            else
                            {
                                CountProgress((long)delta, targetedROI);
                            }
                        }
                        #endregion
                        break;
                }
            }
        }

        /// <summary>
        /// Check for gestures in the given frame. Only one gesture is
        /// supported: a swipe to the left.
        /// </summary>
        /// 
        /// <returns><c>true</c>, if a swipe left gesture was detected, <c>false</c> otherwise.</returns>
        private int SwipeLeft(Frame frame) {
            GestureList gestures = frame.Gestures();

            List<Hand> hands = DetectedHands(frame.Hands);
            List<Finger> fingers = ExtendedFingers(hands[0].Fingers, false);

            if (gestures.Count > 0) {
                foreach (Gesture gesture in gestures) {
                    if (gesture.Type == Gesture.GestureType.TYPESWIPE && gesture.State == Gesture.GestureState.STATESTOP) {
                        SwipeGesture swipe = new SwipeGesture(gesture);

                        if (swipe.Speed >= SWIPE_SPEED && fingers.Count == 2 && swipe.Direction.x <= SWIPE_LEFT_DIRECTION) {
                            Log("Gesture: Back Gesture");
                            int gestureType = (int)AcceptedGestures.GoBack;
                            return gestureType;
                        }
                        else if (swipe.Speed >= SWIPE_SPEED && swipe.Direction.x <= SWIPE_LEFT_DIRECTION)
                        {
                            Log("Gesture: Swipe left");
                            int gestureType = (int)AcceptedGestures.SwipeLeft;
                            return gestureType;
                        }
                    }
                }
            }
            return INVALID_GESTURE;
        }

        private int SwipeRight(Frame frame) {
            GestureList gestures = frame.Gestures();

            List<Hand> hands = DetectedHands(frame.Hands);
            List<Finger> fingers = ExtendedFingers(hands[0].Fingers, false);

            if (gestures.Count > 0) {
                foreach (Gesture gesture in gestures) {
                    if (gesture.Type == Gesture.GestureType.TYPESWIPE && gesture.State == Gesture.GestureState.STATESTOP) {
                        SwipeGesture swipe = new SwipeGesture(gesture);
                        if (swipe.Speed >= SWIPE_SPEED && fingers.Count == 2 && swipe.Direction.x >= SWIPE_RIGHT_DIRECTION) {
                            Log("Gesture: Answer/Finish Call");
                            int gestureType = (int)AcceptedGestures.AnswerCall;
                            return gestureType;
                        }
                        else if (swipe.Speed >= SWIPE_SPEED && swipe.Direction.x >= SWIPE_RIGHT_DIRECTION) {
                            Log("Gesture: Swipe Right");
                            int gestureType = (int)AcceptedGestures.SwipeRight;
                            return gestureType;
                        }
                    }
                }
            }
            return INVALID_GESTURE;
        }

        private int SwipeUp(Frame frame)
        {
            GestureList gestures = frame.Gestures();

            List<Hand> hands = DetectedHands(frame.Hands);
            List<Finger> fingers = ExtendedFingers(hands[0].Fingers, false);

            if (gestures.Count > 0)
            {
                foreach (Gesture gesture in gestures)
                {
                    if (gesture.Type == Gesture.GestureType.TYPESWIPE && gesture.State == Gesture.GestureState.STATESTOP)
                    {
                        SwipeGesture swipe = new SwipeGesture(gesture);

                        if (swipe.Speed >= POWER_SWIPE && fingers.Count == 2 && swipe.Direction.y >= SWIPE_UP_DIRECTION)
                        {
                            Log("Gesture: Open Both Windows");
                            int gestureType = (int)AcceptedGestures.BothOpen;
                            return gestureType; 
                        }
                        
                        else if (swipe.Speed >= POWER_SWIPE && swipe.Direction.y >= SWIPE_UP_DIRECTION)
                        {
                            if (swipe.Position.x >= 0)
                            {
                                Log("Gesture: Open Driver Window");
                                int gestureType = (int)AcceptedGestures.DriverOpen;
                                return gestureType;
                            }
                            else if (swipe.Position.x < 0)
                            {
                                Log("Gesture: Open Passenger Window");
                                int gestureType = (int)AcceptedGestures.PassengerOpen;
                                return gestureType;
                            }
                          
                        }
                        
                        else if (swipe.Speed >= SWIPE_SPEED && swipe.Direction.y >= SWIPE_UP_DIRECTION)
                        {
                            Log("Gesture: Swipe Up");
                            int gestureType = (int)AcceptedGestures.SwipeUp;
                            return gestureType;
                        }
                    }
                }
            }
            return INVALID_GESTURE;
        }

        private int SwipeDown(Frame frame)
        {
            GestureList gestures = frame.Gestures();

            List<Hand> hands = DetectedHands(frame.Hands);
            List<Finger> fingers = ExtendedFingers(hands[0].Fingers, false);

            if (gestures.Count > 0)
            {
                foreach (Gesture gesture in gestures)
                {
                    if (gesture.Type == Gesture.GestureType.TYPESWIPE && gesture.State == Gesture.GestureState.STATESTOP)
                    {
                        SwipeGesture swipe = new SwipeGesture(gesture);

                        if (swipe.Speed >= POWER_SWIPE && fingers.Count == 2 && swipe.Direction.y <= SWIPE_DOWN_DIRECTION)
                        {
                            Log("Gesture: Close Both Windows");
                            int gestureType = (int)AcceptedGestures.BothClosed;
                            return gestureType;
                        }

                        else if (swipe.Speed >= POWER_SWIPE && swipe.Direction.y <= SWIPE_DOWN_DIRECTION)
                        {
                            if (swipe.Position.x >= 0)
                            {
                                Log("Gesture: Open Driver Window");
                                int gestureType = (int)AcceptedGestures.DriverClosed;
                                return gestureType;
                            }
                            else if (swipe.Position.x < 0)
                            {
                                Log("Gesture: Open Passenger Window");
                                int gestureType = (int)AcceptedGestures.PassengerClosed;
                                return gestureType;
                            }

                        }
                        
                        else if (swipe.Speed >= SWIPE_SPEED && swipe.Direction.y <= SWIPE_DOWN_DIRECTION)
                        {
                            Log("Gesture: Swipe Down");
                            int gestureType = (int)AcceptedGestures.SwipeDown;
                            return gestureType;
                        }
                    }
                }
            }
            return INVALID_GESTURE;
        }

        private int ZoomOut(Frame frame)
        {
            GestureList gestures = frame.Gestures();

            if (gestures.Count > 0)
            {
                foreach (Gesture gesture in gestures)
                {
                    if (gesture.Type == Gesture.GestureType.TYPESWIPE && gesture.State == Gesture.GestureState.STATESTOP)
                    {
                        SwipeGesture swipe = new SwipeGesture(gesture);
                        if (swipe.Speed >= SWIPE_SPEED && swipe.Direction.z >= ZOOM_OUT_DIRECTION)
                        {
                            Log("Gesture: Zoom Out");
                            int gestureType = (int)AcceptedGestures.SwipeOut;
                            return gestureType;
                        }
                    }
                }
            }
            return INVALID_GESTURE;
        }

        private int Rotate(Frame frame) {
            GestureList gestures = frame.Gestures();

            if (gestures.Count > 0) {
                foreach (Gesture gesture in gestures) {
                    if (gesture.Type == Gesture.GestureType.TYPECIRCLE && gesture.State == Gesture.GestureState.STATESTOP) {
                        CircleGesture circle = new CircleGesture(gesture);
                        if (circle.Pointable.Direction.AngleTo(circle.Normal) <= Math.PI / 4) {
                            Log("Gesture: Clockwise Rotation");
                            int gestureType = (int)AcceptedGestures.RotateClockwise;
                            return gestureType;
                        }
                        else
                        {
                            Log("Gesture: Anti-Clockwise Rotation");
                            int gestureType = (int)AcceptedGestures.SwipeOut;
                            return gestureType;
                        }
                    }
                }
            }
            return INVALID_GESTURE;
        }

       private int SelectOption(Frame frame) {
            GestureList gestures = frame.Gestures();

            if (gestures.Count > 0) {
                foreach (Gesture gesture in gestures)
                {
                    if (gesture.Type == Gesture.GestureType.TYPESCREENTAP && gesture.State == Gesture.GestureState.STATESTOP){
                        ScreenTapGesture screenTap = new ScreenTapGesture(gesture);
                        Log("Gesture: Screen Tap");
                        int gestureType = (int)AcceptedGestures.SwipeOut;
                        return gestureType;
                    }
                }
            }
            return INVALID_GESTURE;
        }

        /// <summary>
        /// Check for group switches based on the
        /// position of the given hand. Returns true
        /// if a group switch has taken place.
        /// </summary>
        private bool ProcessGroups(Hand hand)
        {
            int midZ = base.workspace.MidZ();
            float palmZ = hand.PalmPosition.z;

            // Determine which group we're in
            int g = (palmZ <= midZ)
                ? 1 : 2;

            if (activeSet.HasGroups)
            {
                if (activeGroup != g)
                {
                    GroupLeave(activeGroup);
                    GroupEnter(g);
                    return true;
                }
            }

            return false;
        }

        private void ShowEdges(GestureSpace space, int group, Hand hand, List<Finger> fingers)
        {
            if (hand == null || fingers == null || fingers.Count == 0)
                return;

            Vector avgPos = AveragePos(fingers);
            int midZ = space.MidZ();
            int z = (int) hand.PalmPosition.z;
            int edgeToShow = 0;

            if (z > midZ - EDGE_THRESHOLD && group == 1)
            {
                edgeToShow = 4;
            }
            else if (z < midZ + EDGE_THRESHOLD && group == 2)
            {
                edgeToShow = 2;
            }

            foreach (ICountObserver observer in observers)
            {
                if (workspace != null)
                {
                    observer.CursorUpdate(workspace.Normalise(avgPos), fingers.Count, edgeToShow);
                }
            }

        }
        #endregion

        #region Events
        private void EnterWorkspace(int handId, int fingers)
        {
            state = CountState.Tracking;
            selectionState = CountSelectionState.Idle;
            activeGroup = 0;
            activeHandId = handId;

            foreach (ICountObserver observer in observers)
                observer.EnterWorkspace(-1, fingers);

            Log("Count: EnterWorkspace");
        }

        private void LeaveWorkspace()
        {
            if (selectionState != CountSelectionState.Idle)
                CountStop();

            GroupLeave(activeGroup);

            state = CountState.Idle;
            selectionState = CountSelectionState.Idle;
            activeGroup = 0;
            discardedFrames = 0;
            activeHandId = -1;

            foreach (ICountObserver observer in observers)
                observer.LeaveWorkspace(0);

            Log("Count: LeaveWorkspace");
        }

        private void PoseStart()
        {
            state = CountState.Ready;
            selectionState = CountSelectionState.Idle;

            Log("Count: PoseStart");
        }

        private void PoseEnd()
        {
            if (selectionState != CountSelectionState.Idle)
                CountStop();

            GroupLeave(activeGroup);

            state = CountState.Tracking;
            selectionState = CountSelectionState.Idle;
            activeGroup = 0;
            discardedFrames = 0;

            Log("Count: PoseEnd");
        }

        private void CountStart(ROI.ROI roi, int count)
        {
            state = CountState.Counting;
            selectionState = CountSelectionState.InProgress;

            countStart = DateTime.Now;
            countTotal = 0;
            countFrames = 0;

            foreach (ICountObserver observer in observers)
                observer.CountStart(null, roi, count);

            Log(String.Format("Count: CountStart {0}", count));
        }

        private void CountStop()
        {
            selectionState = CountSelectionState.Idle;
            countTotal = 0;
            countFrames = 0;
            discardedFrames = 0;

            foreach (ICountObserver observer in observers)
                observer.CountStop();

            // Reset selection progress
            CountProgress(0, null);

            Log("Count: CountStop");
        }

        private void CountSelect(ROI.ROI roi, DateTime time, int count)
        {
            selectionState = CountSelectionState.Complete;
            countSelectedAt = time;

            roi = FindROIWithinGroup(count);

            foreach (ICountObserver observer in observers)
                observer.CountComplete(null, roi, time, count);

            // Triger a ROI set activation
            if (roi.ToActivate != null && roi.ToActivate.Length > 0)
            {
                ActivateROISet(roi.ToActivate);
                Log(String.Format("Activating ROI set: {0}", roi.ToActivate));
            }

            Log(String.Format("Count: CountSelect {0}", count));
        }

        private void CountProgress(long time, ROI.ROI roi)
        {
            foreach (ICountObserver observer in observers)
                observer.CountProgress(time, roi);
        }

        private void GroupLeave(int group)
        {
            if (group == 0)
                return;

            //if (selectionState != CountSelectionState.Idle)
                CountStop();

            activeGroup = 0;

            foreach (ICountObserver observer in observers)
                observer.GroupLeave(group.ToString());

            Log(String.Format("Count: GroupLeave {0}", group));
        }

        private void GroupEnter(int group)
        {
            if (group == 0)
                return;

            activeGroup = group;

            foreach (ICountObserver observer in observers)
                observer.GroupEnter(group.ToString());

            Log(String.Format("Count: GroupEnter {0}", group));
        }
        #endregion

        #region ICountObservers
        public void RegisterObserver(ICountObserver observer)
        {
            observers.Add(observer);
        }

        public void UnregisterObserver(ICountObserver observer)
        {
            observers.Remove(observer);
        }
        #endregion

        #region ROI
        private ROI.ROI FindROIWithinGroup(int count)
        {
            if (activeSet == null)
                return null;

            if (activeSet.HasGroups)
            {
                foreach (ROI.ROI r in activeSet.Regions)
                    if (r.Group == activeGroup && r.Number == count)
                        return r;
            }
            else
            {
                foreach (ROI.ROI r in activeSet.Regions)
                    if (r.Number == count)
                        return r;
            }

                return null;
        }
        #endregion
    }
}
