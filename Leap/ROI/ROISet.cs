using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leap.ROI
{
    public class ROISet
    {
        private List<ROI> rois;
        private String name;
        private bool hasGroups;

        public ROISet()
        {
            this.rois = new List<ROI>();
            this.name = "";
            this.hasGroups = false;
        }

        public ROISet(List<ROI> rois, String name, bool hasGroups)
        {
            this.rois = rois;
            this.name = name;
            this.hasGroups = hasGroups;
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<ROI> Regions
        {
            get { return rois; }
            set { rois = value; }
        }

        public Boolean HasGroups
        {
            get { return hasGroups; }
            set { hasGroups = value; }
        }
    }
}
