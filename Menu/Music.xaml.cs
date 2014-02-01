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
    /// Interaction logic for Music.xaml
    /// </summary>
    public partial class Music : UserControl, IGestureObserver
    {
        
        public Music(Window window, int count)
        {
            InitializeComponent();
            Back();
            NextSong();
            PreviousSong();
            TurnUp();
            TurnDown();
            Interact();
        }

        private void gestureComplete(CountDetector.AcceptedGestures type){
            switch(type){
                case CountDetector.AcceptedGestures.GoBack:
                    Back();
                    break;
                case CountDetector.AcceptedGestures.SwipeLeft:
                    NextSong();
                    break;
                case CountDetector.AcceptedGestures.SwipeRight:
                    PreviousSong();
                    break;
                case CountDetector.AcceptedGestures.RotateClockwise:
                    TurnUp();
                    break;
                case CountDetector.AcceptedGestures.RotateAntiCLockwise:
                    TurnDown();
                    break;
                case CountDetector.AcceptedGestures.SelectOption:
                    Interact();
                    break;
                default:
                    break;
                }
        }

        private void Back()
        {
            throw new NotImplementedException();
        }
        
        private void NextSong()
        {
            throw new NotImplementedException();
        }

        private void PreviousSong()
        {
            throw new NotImplementedException();
        }

        private void TurnUp()
        {
            throw new NotImplementedException();
        }

        private void TurnDown()
        {
            throw new NotImplementedException();
        }

        private void Interact()
        {
            throw new NotImplementedException();
        }
        
    }
    
}
