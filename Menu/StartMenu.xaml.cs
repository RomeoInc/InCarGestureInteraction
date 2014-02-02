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
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu : UserControl, ICountObserver
    {
        public StartMenu()
        {
            InitializeComponent();
        }

        public void EnterWorkspace(int hands, int fingers) { }
        public void LeaveWorkspace(int dummyToAllowOverriding) { }

        // Count selection updates
        public void CountStart(Leap.Vector pos, ROI roi, int count) { }
        public void CountStop() { }
        public void CountComplete(Leap.Vector pos, ROI roi, DateTime time, int count) { }
        public void CountProgress(long dwellTime, ROI roi) { }

        // Cursor position update
        public void CursorUpdate(Leap.Vector pos, int count, int edge) { }

        // Tile group updates
        public void GroupEnter(String name) { }
        public void GroupLeave(String name) { }

        public void Back() { }

    }
}
