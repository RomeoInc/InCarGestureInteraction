using System;
using System.Drawing;

namespace Leap.ROI
{
    interface IROIListener
    {
        void RegisterROISet(ROISet rois);

        void ActivateROISet(String name);

        ROI FindROI(Point point);
    }
}
