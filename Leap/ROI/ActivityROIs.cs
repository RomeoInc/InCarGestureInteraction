using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leap.ROI
{
    class ActivityROIs
    {
        public static void ConnectROIs(GestureDetector gestureDetector)
        {
            // Activity: MainActivity
            List<ROI> mainROIs = new List<ROI>();
            mainROIs.Add(ROIData.CreateROI("Music", 1, 1, "", 1, 1));
            mainROIs.Add(ROIData.CreateROI("GPS", 2, 1, "", 1, 2));
            mainROIs.Add(ROIData.CreateROI("Contacts", 3, 1, "", 2, 1));
            mainROIs.Add(ROIData.CreateROI("Extras", 4, 1, "", 2, 2));

            ROISet main = new ROISet(mainROIs, "StartMenu", true);
            gestureDetector.RegisterROISet(main);

        }
    }
}
