using System;
using System.Drawing;

namespace Leap.ROI
{
    public class ROI
    {
        private String name;
        private int number;
        private int group;
        private Rectangle rect;
        private String toActivate;

        public ROI(Rectangle rect, String name, int number, int group, String toActivate)
        {
            this.name = name;
            this.number = number;
            this.group = group;
            this.rect = rect;
            this.toActivate = toActivate;
        }

        #region Rectangle
        public int Top
        {
            get { return rect.Top; }
        }

        public int Bottom
        {
            get { return rect.Bottom; }
        }

        public int Left
        {
            get { return rect.Left; }
        }

        public int Right
        {
            get { return rect.Right; }
        }

        public int Height
        {
            get { return rect.Height; }
        }

        public int Width
        {
            get { return rect.Width; }
        }
        #endregion

        #region Metadata
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        public int Group
        {
            get { return group; }
            set { group = value; }
        }

        public String ToActivate
        {
            get { return toActivate; }
            set { toActivate = value; }
        }
        #endregion

        #region Equals
        public override bool Equals(Object obj)
        {
            ROI roi = obj as ROI;
            if ((object)roi == null)
            {
                return false;
            }

            // Return true if the fields match:
            return roi.Name == name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public static bool operator ==(ROI a, ROI b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(ROI a, ROI b)
        {
            return !(a == b);
        }
        #endregion
    }
}
