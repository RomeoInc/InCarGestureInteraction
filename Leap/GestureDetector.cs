using System;
using Leap;
using System.Collections.Generic;
using Leap.ROI;
using System.Drawing;

namespace Leap
{
    /// <summary>
    /// This is a base class for a gesture detector. Deriving classes
    /// should implement the FrameUpdate function to use Frames from
    /// the Leap device to track interaction.
    /// 
    /// A GestureSpace instance can be registered with this class to
    /// limit interaction to a certain area detectable by the Leap. It
    /// is the deriving class's responsibility to perform this check.
    /// </summary>
    public abstract class GestureDetector : IFrameListener, IROIListener
    {
        public static bool DEBUG = false; // If true, Log will print to Console
        public const int MIN_FINGER_LENGTH = 15; // Minimum finger length in mm
        public const int SPACE_PADDING = 200;
        public const int MIN_HEIGHT = 40; // Fingers will be ignored when 4 cm above the display

        /// <summary>
        /// Fingers are rejected if the absolute y component of the
        /// finger direction exceeds this value.
        /// </summary>
        public const float Y_THRESHOLD = 0.3f;

        protected GestureSpace workspace;

        // Leap workspace regions of interest
        protected List<ROISet> roiSets;
        protected ROISet activeSet;

        public GestureDetector()
        {
            roiSets = new List<ROISet>();
        }

        #region IDwellROI
        public void RegisterROISet(ROISet rois)
        {
            roiSets.Add(rois);

            if (activeSet == null)
                activeSet = rois;
        }

        public void ActivateROISet(String name)
        {
            foreach (ROISet rois in roiSets)
            {
                if (rois.Name.Equals(name))
                {
                    activeSet = rois;

                    return;
                }
            }
        }

        /// <summary>
        /// Find the region of interest containing the given point, or null if
        /// no region is found.
        /// </summary>
        public ROI.ROI FindROI(Point point)
        {
            if (activeSet == null)
                return null;

            foreach (ROI.ROI r in activeSet.Regions)
            {
                if (r.Top <= point.Y && r.Bottom >= point.Y && r.Left <= point.X && r.Right >= point.X)
                {
                    return r;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the region of interest with the given name.
        /// </summary>
        public ROI.ROI FindROI(String name)
        {
            if (activeSet == null)
                return null;

            foreach (ROI.ROI r in activeSet.Regions)
            {
                if (r.Name.Equals(name))
                {
                    return r;
                }
            }

            return null;
        }
        #endregion */

        #region Leap
        /// <summary>
        /// Process a frame update from the Leap interface.
        /// </summary>
        /// 
        /// <param name="frame">Leap frame to process.</param>
        public abstract void FrameUpdate(Frame frame);

        /// <summary>
        /// Returns the list of fingers which exceed a certain minimum
        /// length. This provides a best guess of which fingers the user
        /// intended to be visible.
        /// </summary>
        /// 
        /// <param name="fingers">List of fingers detected in a Leap frame.</param>
        /// <param name="limitSpace">If true, only accept fingers in the gesture space.</param> 
        protected List<Finger> ExtendedFingers(FingerList fingers, bool limitSpace)
        {
            List<Finger> extended = new List<Finger>();

            foreach (Finger finger in fingers)
            {
                Vector tip = finger.TipPosition;

                if (finger.Length >= MIN_FINGER_LENGTH &&
                    tip.y >= MIN_HEIGHT &&
                    (limitSpace == false && Math.Abs(finger.Direction.y) < Y_THRESHOLD ||
                    (tip.z < GestureSpace.Bottom + SPACE_PADDING &&
                    tip.x > GestureSpace.Left - SPACE_PADDING &&
                    tip.x < GestureSpace.Right + SPACE_PADDING)))
                {
                    extended.Add(finger);
                }
            }

            return extended;
        }

        /// <summary>
        /// Returns a list of hands which fall within a certain space around the gesture workspace.
        /// </summary>
        protected List<Hand> DetectedHands(HandList hands)
        {
            List<Hand> detected = new List<Hand>();

            foreach (Hand hand in hands)
            {
                if (hand.PalmPosition.z < GestureSpace.Bottom + SPACE_PADDING)
                    detected.Add(hand);
            }

            return detected;
        }

        /// <summary>
        /// Calculates the average position of the given fingers.
        /// </summary>
        /// 
        /// <param name="fingers">List of fingers detected in a Leap frame.</param>
        protected Vector AveragePos(List<Finger> fingers)
        {
            Vector avgPos = Vector.Zero;

            foreach (Finger finger in fingers)
            {
                avgPos += finger.TipPosition;
            }

            avgPos /= fingers.Count;

            return avgPos;
        }

        protected float MinZ(List<Finger> fingers)
        {
            float minZ = float.MaxValue;

            foreach (Finger finger in fingers)
            {
                if (finger.TipPosition.z < minZ)
                    minZ = finger.TipPosition.z;
            }

            return minZ;
        }
        #endregion

        #region IFrameListener
        public void OnFrame(Frame frame)
        {
            // Pass this update off to the state machine
            FrameUpdate(frame);
        }
        #endregion

        #region Workspace
        /// <summary>
        /// Registers a gesture workspace for this state machine. The
        /// hand is only considered to be in the correct pose if it
        /// is within this area.
        /// </summary>
        public void RegisterWorkspace(GestureSpace workspace)
        {
            this.workspace = workspace;

            Log("Workspace: " + workspace.ToString());
        }
        #endregion

        #region Utility
        protected void Log(String message)
        {
            DateTime now = DateTime.Now;

            if (DEBUG)
                Console.WriteLine("GestureDetector [{0}.{1:000}]: {2}", now.ToLongTimeString(), now.Millisecond, message);
        }
        #endregion
    }
}
