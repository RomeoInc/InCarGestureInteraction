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


    private enum Device {S2, S3, N4};

    private StartMenu StartMenu;

    private const Device DEVICE = Device.S3;
	private const String PHONE_IP = "192.168.0." + (DEVICE == Device.S3 ? "5" : (DEVICE == Device.N4 ? "6" : "3"));
    private const int PHONE_PORT = 54545;
	private const String TABLET_IP = "192.168.0.4";
	private const int TABLET_PORT = 54545;
    private const int PUREDATA_PORT = 34567;

    private static List<NetworkInterface> devices;

    static void Main(string[] args)
    {
		devices = new List<NetworkInterface>();

        #region 1. Read participant number and the current block number
        Print("Participant number: ");
        int participant = Int16.Parse(Console.ReadLine());

        Print("Block number: ");
        int block = Int16.Parse(Console.ReadLine());
        #endregion

        #region 2. Determine which condition to run
        int condition = GetConditionNumber(participant, block);
        GestureType gesture = GetGesture(condition);
        FeedbackType feedback = GetFeedback(condition);

        Print("\n\n-------------------------------------");
        Print("Participant: " + participant);
        Print("Block:       " + block);
        Print("Condition:   " + condition);
        Print("Gesture:     " + gesture);
        Print("Feedback:    " + feedback);
        Print("-------------------------------------\n\n");
        #endregion

        #region 3. Connect to Leap Motion
        LeapInterface leap = new LeapInterface();
        GestureSpace space = new GestureSpace();
        #endregion

        #region 4. Prepare gesture detector
        switch (gesture)
        {
            case GestureType.Point:
                InitialisePoint(leap, space, feedback, participant, block, condition);
                break;
            case GestureType.Count:
                InitialiseCount(leap, space, feedback, participant, block, condition);
                break;
        }
        #endregion

        // Everything happens asynchronously from this point

        #region 5. Wait for input before quitting
        Console.ReadLine();

        leap.Stop();
        leap.Destroy();

		foreach (NetworkInterface device in devices)
            device.Disconnect();
        #endregion
    }

    #region Gesture detectors
    private static void InitialisePoint(LeapInterface leap, GestureSpace space, FeedbackType feedback, int participant, int block, int condition)
    {
        // Create gesture detector
        PointingDetector point = new PointingDetector(leap, space);

        // Register regions of interest
        ActivityROIs.ConnectROIs(point);

        // Visual feedback interface
        PointingVisual visual = new PointingVisual(String.Format("P{0}-B{0}-PointVisual", participant, block));
        visual.Connect(PHONE_IP, PHONE_PORT);
        devices.Add(visual);
        point.RegisterPointingListener(visual);
        point.RegisterSelectionListener(visual);

		// Visual instruction interface
        PointingVisual instruction = new PointingVisual(String.Format("P{0}-B{0}-Instruction", participant, block));
		instruction.Connect(TABLET_IP, TABLET_PORT);
		devices.Add(instruction);

        if (!instruction.Connected)
            instruction = null;

        // Tactile feedback interface
        PointingTactile tactile = new PointingTactile(feedback, String.Format("P{0}-B{0}-PointTactile", participant, block));
        tactile.Connect(PUREDATA_PORT);
        point.RegisterPointingListener(tactile);
        point.RegisterSelectionListener(tactile);

        StartExperiment(participant, block, condition, GestureType.Point, point, visual, instruction, tactile);
    }

    private static void InitialiseCount(LeapInterface leap, GestureSpace space, FeedbackType feedback, int participant, int block, int condition)
    {
        // Create gesture detector
        CountDetector count = new CountDetector(leap, space);

        // Register regions of interest
        ActivityROIs.ConnectROIs(count);

        // Visual feedback interface
        CountVisual visual = new CountVisual(String.Format("P{0}-B{0}-CountVisual", participant, block));
        visual.Connect(PHONE_IP, PHONE_PORT);
        devices.Add(visual);
        count.RegisterObserver(visual);

		// Visual instruction interface
        CountVisual instruction = new CountVisual(String.Format("P{0}-B{0}-Instruction", participant, block));
		instruction.Connect(TABLET_IP, TABLET_PORT);
		devices.Add(instruction);

        if (!instruction.Connected)
            instruction = null;

        // Tactile feedback interface
        CountTactile tactile = new CountTactile(feedback, String.Format("P{0}-B{0}-CountTactile", participant, block));
        tactile.Connect(Metadata.PUREDATA_PORT);
        count.RegisterObserver(tactile);

        visual.RegisterObserver(count);

        StartExperiment(participant, block, condition, GestureType.Count, count, visual, instruction, tactile);
    }
    #endregion

    #region Experiment
	private static void StartExperiment(int participant, int block, int condition, GestureType gesture, GestureDetector gestureDetector, NetworkInterface visual, NetworkInterface instruction, NetworkInterface tactile)
    {
        Block experimentBlock = new Block(participant, block, condition, gesture, gestureDetector, visual, instruction, tactile);

        // Register Block instance to receive messages from phone
        visual.RegisterObserver(experimentBlock);

        switch (gesture)
        {
            case GestureType.Point:
                PointingDetector point = (PointingDetector)gestureDetector;
                point.RegisterPointingListener(experimentBlock);
                point.RegisterSelectionListener(experimentBlock);
                break;
            case GestureType.Count:
                CountDetector count = (CountDetector)gestureDetector;
                count.RegisterObserver(experimentBlock);
                break;
        }

        experimentBlock.StartBlock();
    }

    private static int GetConditionNumber(int participant, int block)
    {
        int n = Metadata.N_CONDITIONS;

        // Get latin square for condition order
        int[,] latinSquare = BalancedLatinSquare.GetLatinSquare(n);

        // Get this participant's condition order
        int[] conditionOrder = new int[n];
        for (int i = 0; i < n; i++)
            conditionOrder[i] = latinSquare[participant % n, i];

        // Return gesture type
        return conditionOrder[block - 1];
    }

    private static GestureType GetGesture(int condition)
    {
        return Metadata.ConditionGesture[condition - 1];
    }

    private static FeedbackType GetFeedback(int condition)
    {
        return Metadata.ConditionFeedback[condition - 1];
    }
    #endregion

    #region Utility
    private static void Print(String message)
    {
        Console.WriteLine("{0}", message);
    }
    #endregion
}