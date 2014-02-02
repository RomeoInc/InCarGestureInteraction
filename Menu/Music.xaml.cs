using Leap.Gestures.Count;
using Leap.ROI;
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
            GoBack();
            NextSong();
            PreviousSong();
            TurnUp();
            TurnDown();
            Interact();
        }

        public void GestureComplete(AcceptedGestures type){
            switch(type){
                case AcceptedGestures.GoBack:
                    GoBack();
                    break;
                case AcceptedGestures.SwipeLeft:
                    NextSong();
                    break;
                case AcceptedGestures.SwipeRight:
                    PreviousSong();
                    break;
                case AcceptedGestures.RotateClockwise:
                    TurnUp();
                    break;
                case AcceptedGestures.RotateAntiCLockwise:
                    TurnDown();
                    break;
                case AcceptedGestures.SelectOption:
                    Interact();
                    break;
                default:
                    break;
                }
        }

        private void GoBack()
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
