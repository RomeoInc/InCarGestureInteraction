using Leap.Gestures.Count;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LeapPointer_PC.Menu
{
    /// <summary>
    /// Interaction logic for GPS.xaml
    /// </summary>
    public partial class GPS : UserControl, IGestureObserver 
    {
        public GPS()
        {
            InitializeComponent();
            Back();
            ScrollLeft();
            ScrollRight();
            ScrollUp();
            ScrollDown();
            ZoomIn();
            ZoomOut();
            PlaceMarker();
        }

        private void gestureComplete(CountDetector.AcceptedGestures type)
        {
            switch (type)
            {
                case CountDetector.AcceptedGestures.GoBack:
                    Back();
                    break;
                case CountDetector.AcceptedGestures.SwipeLeft:
                    ScrollLeft();
                    break;
                case CountDetector.AcceptedGestures.SwipeRight:
                    ScrollRight();
                    break;
                case CountDetector.AcceptedGestures.SwipeUp:
                    ScrollUp();
                    break;
                case CountDetector.AcceptedGestures.SwipeDown:
                    ScrollDown();
                    break;
                case CountDetector.AcceptedGestures.SwipeIn:
                    ZoomIn();
                    break;
                case CountDetector.AcceptedGestures.SwipeOut:
                    ZoomOut();
                    break;
                case CountDetector.AcceptedGestures.SelectOption:
                    PlaceMarker();
                    break;
                default:
                    break;
            }
        }

        private void Back()
        {
            throw new NotImplementedException();
        }

        private void ScrollLeft()
        {
            throw new NotImplementedException();
        }

        private void ScrollRight()
        {
            throw new NotImplementedException();
        }

        private void ScrollUp()
        {
            throw new NotImplementedException();
        }

        private void ScrollDown()
        {
            throw new NotImplementedException();
        }

        private void ZoomIn()
        {
            throw new NotImplementedException();
        }

        private void ZoomOut()
        {
            throw new NotImplementedException();
        }

        private void PlaceMarker()
        {
            throw new NotImplementedException();
        }
    }
}
