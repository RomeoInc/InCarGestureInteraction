using System;

namespace Leap.Gestures.Pointing
{
    public interface ISelectionObserver
    {
        void StartDwell(Vector pos, ROI.ROI roi);

        void StopDwell();

        void DwellSelect(Vector pos, ROI.ROI roi, DateTime time);

        void ProgressUpdate(long dwellTime);
    }
}