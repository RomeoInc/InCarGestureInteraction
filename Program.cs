using System;
using System.Collections.Generic;
using System.Threading;
using Leap;
using Experiment;
using Interfaces;
using Leap.Gestures.Pointing;
using Leap.Gestures.Pointing.Feedback;
using Leap.ROI;
using FeedbackType = Experiment.Metadata.FeedbackType;
using GestureType = Experiment.Metadata.GestureType;
using Leap.Gestures.Count;
using Leap.Gestures.Count.Feedback;
using LeapPointer_PC.Menu;


class Program
{
    

    static void Main(string[] args)
    {
		StartMenu menu = new StartMenu();
        LeapInterface leap = new LeapInterface();
        GestureSpace space = new GestureSpace();
         
        InitialiseCount(leap, space, menu);

        Console.ReadLine();

        leap.Stop();
        leap.Destroy();
    }

    #region Gesture detectors
    private static void InitialiseCount(LeapInterface leap, GestureSpace space, StartMenu menu)
    {
        // Create gesture detector
        CountDetector count = new CountDetector(leap, space);

        // Register regions of interest
        ActivityROIs.ConnectROIs(count);
        count.RegisterObserver(menu); 
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }
    #endregion

        #region Utility
    private static void Print(String message)
    {
        Console.WriteLine("{0}", message);
    }
    #endregion
}