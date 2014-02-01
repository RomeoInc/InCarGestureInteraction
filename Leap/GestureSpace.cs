using System;
using System.Drawing;
using Leap;

namespace Leap
{
    public class GestureSpace
    {
        #region Constants
        /// <summary>
        /// Width of the gesture space, in mm.
        /// </summary>
        public const int Width = 80;

        /// <summary>
        /// Height of the gesture space, in mm.
        /// </summary>
        public const int Height = 80;

        /// <summary>
        /// Left boundary of the gesture space, relative to the Leap origin.
        /// </summary>
        public static int Left = -40;

        /// <summary>
        /// Right boundary of the gesture space, relative to the Leap origin.
        /// </summary>
        public static int Right = Left + Width;

        /// <summary>
        /// Lower boundary of the gesture space, relative to the Leap origin.
        /// </summary>
        public static int Bottom = -20;

        /// <summary>
        /// Upper boundary of the gesture space, relative to the Leap origin.
        /// </summary>
        public static int Top = Bottom - Height;
        #endregion

        private Rectangle xz;

        /// <summary>
        /// Initializes a new instance of the <see cref="Leap.GestureSpace"/> class
        /// using the predefined constants to define the workspace size.
        /// </summary>
        public GestureSpace()
        {
            Vector topLeft = new Vector(Left, 0, Top);
            Vector bottomRight = new Vector(Right, 0, Bottom);

            xz = new Rectangle((int)topLeft.x, (int)topLeft.z, (int)(bottomRight.x - topLeft.x), (int)Math.Abs(topLeft.z - bottomRight.z));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Leap.GestureSpace"/> class
        /// using the given boundaries.
        /// </summary>
        /// <param name="left">Left edge of the workspace, defined in mm relative to Leap.</param>
        /// <param name="top">Top edge of the workspace, defined in mm relative to Leap.</param>
        /// <param name="right">Right edge of the workspace, defined in mm relative to Leap.</param>
        /// <param name="bottom">Bottom edge of the workspace, defined in mm relative to Leap.</param>
        public GestureSpace(int left, int top, int right, int bottom)
        {
            Vector topLeft = new Vector(left, 0, top);
            Vector bottomRight = new Vector(right, 0, bottom);

            xz = new Rectangle((int)topLeft.x, (int)topLeft.z, (int)(bottomRight.x - topLeft.x), (int)Math.Abs(topLeft.z - bottomRight.z));
        }

        /// <summary>
        /// Returns true if the current workspace contains the given point.
        /// </summary>
        /// 
        /// <param name="point">Point to check.</param>
        /// 
        /// <returns>True if point is within the workspace.</returns>
        public bool Contains(Vector point)
        {
            return point.x >= xz.Left && point.x <= xz.Right
                && point.z >= xz.Top && point.z <= xz.Bottom;
        }

        /// <summary>
        /// Gives the position of the given point within the workspace
        /// mapped to the range of 0 to 1 for each component, showing the
        /// relative position within the workspace.
        /// </summary>
        public Vector Normalise(Vector point)
        {
            float x = (point.x + Math.Abs(xz.Left)) / xz.Width;
            float z = (point.z + Math.Abs(xz.Top)) / xz.Height;

            return new Vector(x, point.y, z);
        }

        /// <summary>
        /// Gives the middle z coordinate.
        /// </summary>
        public int MidZ()
        {
            return xz.Top + (xz.Height / 2);
        }

        /// <summary>
        /// Gives a Rectangle object representing the workspace
        /// in the xz plane.
        /// </summary>
        public Rectangle GetRectangle()
        {
            return xz;
        }

        public override String ToString()
        {
            return String.Format("({0},{1}) ({2},{3})", xz.X, xz.Y, xz.Right, xz.Bottom);
        }
    }
}
