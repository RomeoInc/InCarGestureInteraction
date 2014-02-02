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
    /// Interaction logic for Contacts.xaml
    /// </summary>
    public partial class Contacts : UserControl, IGestureObserver
    {
        public Contacts()
        {
            InitializeComponent();
            Back();
            NextPerson();
            PreviousPerson();
            Call();
            HangUp();
            Reject();
        }

        private void Back()
        {
            throw new NotImplementedException();
        }

        private void NextPerson()
        {
            throw new NotImplementedException();
        }

        private void PreviousPerson()
        {
            throw new NotImplementedException();
        }

        private void Call()
        {
            throw new NotImplementedException();
        }

        private void HangUp()
        {
            throw new NotImplementedException();
        }

        private void Reject()
        {
            throw new NotImplementedException();
        }
    }
}
