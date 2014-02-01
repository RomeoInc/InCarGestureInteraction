using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leap.Gestures.Count
{
    interface IGestureObserver : ICountObserver
    {
        public void gestureComplete(int type);
    }
}
