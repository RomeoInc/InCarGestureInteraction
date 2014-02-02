using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leap.Gestures.Count
{
    public interface ICountObserver
    {
        // Workspace updates
        void EnterWorkspace(int hands, int fingers);
        void LeaveWorkspace(int dummyToAllowOverriding);

        // Count selection updates
        void CountStart(Vector pos, ROI.ROI roi, int count);
        void CountStop();
        void CountComplete(Vector pos, ROI.ROI roi, DateTime time, int count);
        void CountProgress(long dwellTime, ROI.ROI roi);

        // Cursor position update
        void CursorUpdate(Vector pos, int count, int edge);

        // Tile group updates
        void GroupEnter(String name);
        void GroupLeave(String name);

        // Go back to previous activity
        void Back();
    }
}
