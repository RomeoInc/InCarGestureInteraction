using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leap.Gestures.Count
{
    public interface IGestureObserver
    {
        void GestureComplete(AcceptedGestures type);
    }
}
