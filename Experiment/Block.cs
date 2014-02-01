using System;
using System.Collections.Generic;
using System.Threading;
using Experiment.Tasks;
using Interfaces;
using Leap;
using Leap.Gestures.Pointing;
using Leap.ROI;
using GestureType = Experiment.Metadata.GestureType;
using Leap.Gestures.Count;

namespace Experiment
{
    class Block : ISelectionObserver, IPointingObserver, ICountObserver, INetworkObserver
    {
        #region Block details
        private int participantNum;
        private int blockNum;
        private int conditionNum;
        #endregion

        #region Tasks
        /// <summary>
        /// The active experiment task.
        /// </summary>
        private Task activeTask;

        /// <summary>
        /// The number of the active experiment task.
        /// </summary>
        private int activeTaskNumber;

        /// <summary>
        /// A collection of all tasks in the experiment.
        /// </summary>
        private List<Task> tasks;
        #endregion

        #region Active task stages
        /// <summary>
        /// The name of the active ROI set.
        /// </summary>
        private String activeSet;

        /// <summary>
        /// The name of the previous active ROI set.
        /// </summary>
        private String previousActiveSet;

        /// <summary>
        /// The active stage of the current experiment task.
        /// </summary>
        private SelectionTaskStage activeTaskStage;

        /// <summary>
        /// The number of the active task stage.
        /// </summary>
        private int activeTaskStageNumber;
        #endregion

        #region Metrics
        /// <summary>
        /// DateTime at which the cursor entered the selection target.
        /// </summary>
        private DateTime enterTarget;

        /// <summary>
        /// If true, the cursor is on the target.
        /// </summary>
        private bool onTarget;

        /// <summary>
        /// Number of times the hand left stopped pointing - either due to
        /// leaving the gesture space, or leaving the pointing pose.
        /// </summary>
        private int stoppedPointing;

        /// <summary>
        /// Number of times the cursor fell off the target whilse dwelling.
        /// </summary>
        private int slips;

        /// <summary>
        /// Number of times the user made an incorrect selection during the active stage.
        /// </summary>
        private int incorrectSelections;

        #region Pointing time
        /// <summary>
        /// Time at which the user last resumed pointing.
        /// </summary>
        private DateTime? startedPointing;

        /// <summary>
        /// Total time spent pointing.
        /// </summary>
        private long timeSpentPointing;
        #endregion

        #region Selection time
        /// <summary>
        /// DateTime at which the hand entered the view of the Leap.
        /// </summary>
        private DateTime? enterWorkspace;

        /// <summary>
        /// If true, the hand has already initially entered the workspace.
        /// </summary>
        private bool enteredWorkspaceThisStage;

        /// <summary>
        /// Time taken from first entering the workspace to completing a selection.
        /// </summary>
        private long selectionTime;
        #endregion

        #region Time to start pointing
        /// <summary>
        /// Time taken from hand being spotted to beginning pointing.
        /// </summary>
        private long timeToPointing;
        #endregion

        #region Time to target
        /// <summary>
        /// Time taken to start dwelling on the target.
        /// </summary>
        private long timeToTarget;
        #endregion
        #endregion

        #region Gesture interface
        private GestureType gesture;
        private bool isUIEnabled;
        private NetworkInterface visualInterface;
        private NetworkInterface instructionInterface;
        private NetworkInterface tactileInterface;
        private GestureDetector gestureDetector;
        #endregion

		public Block(int participant, int block, int condition, GestureType gesture, GestureDetector gestureDetector, NetworkInterface visualInterface, NetworkInterface instructionInterface, NetworkInterface tactileInterface)
        {
            // Block metadata
            this.participantNum = participant;
            this.blockNum = block;
            this.conditionNum = condition;
            this.gesture = gesture;

            // Communication
            this.gestureDetector = gestureDetector;
            this.visualInterface = visualInterface;
            this.tactileInterface = tactileInterface;
            this.instructionInterface = instructionInterface;

            // Metrics
            isUIEnabled = false;

            InitialiseTasks();
        }

        #region INetworkObserver
        public void MessageReceived(String message) {
            Console.WriteLine("Message received: " + message);

            if (message.Equals("back"))
                Back();
        }
        #endregion

        /// <summary>
        /// Initialise tasks for this block.
        /// </summary>
        private void InitialiseTasks()
        {
            // Initialise task details
            activeSet = Metadata.INITIAL_SET;
            previousActiveSet = null;
            activeTaskStage = null;
            activeTaskStageNumber = 0;
            activeTask = null;
            activeTaskNumber = 0;

            ExperimentTasks expTasks = new ExperimentTasks(2, Metadata.TASKS_PER_BLOCK / 2);
            tasks = expTasks.Tasks;
        }

        #region Enable and disable UI
        /// <summary>
        /// Enable the user interface.
        /// </summary>
        private void EnableUI()
        {
            visualInterface.Send(gesture == GestureType.Point ? Protocol.MessageEnableUIPoint : Protocol.MessageEnableUICount);

            if (tactileInterface != null)
                tactileInterface.Send(Protocol.MessageEnableUI);

            isUIEnabled = true;
        }

        /// <summary>
        /// Disable the user interface.
        /// </summary>
        private void DisableUI()
        {
            visualInterface.Send(Protocol.MessageDisableUI);

            if (tactileInterface != null)
                tactileInterface.Send(Protocol.MessageDisableUI);

            isUIEnabled = false;
        }
        #endregion

        #region Experiment
        /// <summary>
        /// Start the block. Go to the first task within this block.
        /// </summary>
        public void StartBlock()
        {
            LogExperimentData(String.Format("Participant,Block,Condition,Task,Stage,Slips,Stopped,Incorrect,SelectionTime,PointingTime,TimeToStart,TimeToTarget"));

            Log(String.Format("Starting experiment. There are {0} tasks.", tasks.Count));

            // Disable UI at the start
            DisableUI();

            NextExperimentTask();
        }

        /// <summary>
        /// Move to the next task.
        /// </summary>
        public void NextExperimentTask()
        {
            if (activeTaskNumber >= tasks.Count)
            {
                EndBlock();
                return;
            }

            activeSet = "Main";
            previousActiveSet = null;
            activeTaskNumber++;
            activeTask = tasks[activeTaskNumber - 1];

            (new Thread(StartTask)).Start();
        }

        /// <summary>
        /// End of block - i.e., participant has completed all tasks.
        /// </summary>
        public void EndBlock()
        {
            Log("End of block.");

            // Send a restart message to return to the main activity
            visualInterface.Send(Protocol.MessageRestart);

            // Wait a moment... Gives the phone time to return
            Thread.Sleep(1000);

            // Give the participant a message
            String instruction = String.Format("{0}_End of block!", Protocol.MessageShowMessage);
            visualInterface.Send(instruction);
            
            if (instructionInterface != null)
                instructionInterface.Send(instruction);
        }

        /// <summary>
        /// Start the next task.
        /// </summary>
        public void StartTask()
        {
            Console.WriteLine("\n------------------------------------\n");
            Console.WriteLine("Task {0}: {1}\n", activeTaskNumber, activeTask.Description);

            // Send a message to instruct the user
            String instruction = String.Format("{0}_{1}", Protocol.MessageShowMessage, activeTask.Description);
            visualInterface.Send(instruction);
            
            if (instructionInterface != null)
                instructionInterface.Send(instruction);

            // Wait a couple of seconds then enable the UI
            Thread.Sleep(1000);
            EnableUI();

            activeTaskStageNumber = 0;
            NextTaskStage();
        }

        /// <summary>
        /// Move to the next part of the task.
        /// </summary>
        public void NextTaskStage()
        {
            if (activeTaskStageNumber >= activeTask.Stages)
            {
                EndTaskStage();
                EndTask();
                return;
            }
            else if (activeTaskStageNumber > 0)
            {
                EndTaskStage();
            }

            activeTaskStageNumber++;
            activeTaskStage = (SelectionTaskStage)activeTask[activeTaskStageNumber - 1];
            activeTaskStage.Start();

            // Reset metrics
            slips = 0;
            stoppedPointing = 0;
            incorrectSelections = 0;
            onTarget = false;

            // Pointing time
            startedPointing = null;
            timeSpentPointing = 0;

            // Selection time
            selectionTime = 0;
            enteredWorkspaceThisStage = false;
            enterWorkspace = null;

            // Time to start pointing
            timeToPointing = 0;

            // Time to hit target
            timeToTarget = 0;
        }

        /// <summary>
        /// End of the current task stage.
        /// </summary>
        private void EndTaskStage()
        {
            LogExperimentData(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                participantNum, blockNum, conditionNum,                     // Meta-data
                activeTaskNumber, activeTaskStageNumber,                    // Task & stage meta-data
                slips, stoppedPointing, incorrectSelections,                // Events
                selectionTime, timeSpentPointing, timeToPointing,           // Times
                timeToTarget                                                // More times...
            ));
        }

        /// <summary>
        /// End of the current task.
        /// </summary>
        public void EndTask()
        {
            Console.WriteLine("\nEnd of task\n");

            // Send a command to disable UI and return to the start of the app
            DisableUI();
            visualInterface.Send(Protocol.MessageRestart);

            if (instructionInterface != null)
                instructionInterface.Send(String.Format("{0}_ ", Protocol.MessageShowMessage));

            activeTaskStage = null;
            onTarget = false;

            gestureDetector.ActivateROISet(Metadata.INITIAL_SET);

            // Wait for 1 seconds before moving to the next task
            Thread.Sleep(1000);

            NextExperimentTask();
        }

        /// <summary>
        /// Start timing for the selection time metric if it hasn't already started.
        /// </summary>
        private void UpdateSelectionTime()
        {
            if (!enteredWorkspaceThisStage)
            {
                enterWorkspace = DateTime.Now;
                enteredWorkspaceThisStage = true;
            }
        }

        /// <summary>
        /// Increment the time spent pointing.
        /// </summary>
        private void IncrementTimePointing()
        {
            if (startedPointing != null)
            {
                double t = DateTime.Now.Subtract((DateTime)startedPointing).TotalMilliseconds;

                timeSpentPointing += (long)t;
            }
        }
        #endregion

        #region ISelectionObserver
        public void StartDwell(Vector pos, ROI roi)
        {
            if (activeTaskStage == null)
                return;

            if (activeSet.Equals(activeTaskStage.TargetSet) && roi.Name.Equals(activeTaskStage.TargetName))
            {
                onTarget = true;
                enterTarget = DateTime.Now;

                if (enterWorkspace == null)
                {
                    timeToTarget = 0;
                    return;
                }

                if (timeToTarget == 0)
                    timeToTarget = (long)enterTarget.Subtract((DateTime)enterWorkspace).TotalMilliseconds;
            }
            else
            {
                onTarget = false;
            }
        }

        public void StopDwell()
        {
            // Slip if the cursor was on the target, isn't any more, and was on for at least the threshold time
            if (onTarget)
                if (DateTime.Now.Subtract(enterTarget).TotalMilliseconds >= Metadata.SLIP_THRESHOLD)
                    slips++;
        }

        public void DwellSelect(Vector pos, ROI roi, DateTime time)
        {
            if (activeTaskStage == null)
                return;

            if (activeTaskStage.CheckSelection(roi.Name, activeSet))
            {
                // Update the time pointing metric
                IncrementTimePointing();

                // Update the selection time metric
                selectionTime = (long)DateTime.Now.Subtract((DateTime)enterWorkspace).TotalMilliseconds;

                NextTaskStage();
            }
            else
            {
                incorrectSelections++;
            }

            // If this selection starts a new activity, update the active set
            if (roi.ToActivate != null && roi.ToActivate.Length > 0)
                UpdateActiveSet(roi.ToActivate);
        }

        public void ProgressUpdate(long dwellTime)
        {

        }
        #endregion

        #region IPointingObserver
        public void EnterWorkspace()
        {
            UpdateSelectionTime();
        }

        public void LeaveWorkspace()
        {

        }

        public void InitialPose()
        {
            DateTime now = DateTime.Now;

            // Time pointing
            startedPointing = now;

            if (enterWorkspace != null)
                timeToPointing = (long)now.Subtract((DateTime)enterWorkspace).TotalMilliseconds;
            else
                timeToPointing = 0;

            // Selection time
            UpdateSelectionTime();
        }

        public void ExitPose()
        {
            // Stopped pointing
            stoppedPointing++;

            IncrementTimePointing();
        }

        public void StillPointing(Vector pos, DateTime when)
        {
            // Check if we started a task stage already in the pointing pose
            if (startedPointing == null && activeTaskStage != null)
            {
                if (activeTaskStageNumber == 1 && isUIEnabled)
                {
                    // This shouldn't happen, but just in case - make sure the UI is showing
                    visualInterface.Send(gesture == GestureType.Point ? Protocol.MessageEnterWorkspacePoint : Protocol.MessageEnterWorkspaceCount);
                }

                InitialPose();
            }

            // Check if we started a task stage already in the workspace
            if (activeTaskStageNumber > 1)
                UpdateSelectionTime();
        }
        #endregion

        #region ICountObserver
        public void EnterWorkspace(int hands, int fingers)
        {
            UpdateSelectionTime();
        }

        public void LeaveWorkspace(int dummyToAllowOverriding) {}

        public void CountStart(Vector pos, ROI roi, int count)
        {
            if (activeTaskStage == null)
                return;

            if (startedPointing == null)
                startedPointing = DateTime.Now;

            if (activeSet.Equals(activeTaskStage.TargetSet) && roi.Name.Equals(activeTaskStage.TargetName))
            {
                onTarget = true;
                enterTarget = DateTime.Now;

                if (enterWorkspace == null)
                {
                    timeToTarget = 0;
                    return;
                }

                if (timeToTarget == 0)
                    timeToTarget = (long)enterTarget.Subtract((DateTime)enterWorkspace).TotalMilliseconds;
            }
            else
            {
                onTarget = false;
            }
        }

        public void CountStop()
        {
            // Slip if the cursor was on the target, isn't any more, and was on for at least the threshold time
            if (onTarget)
                if (DateTime.Now.Subtract(enterTarget).TotalMilliseconds >= Metadata.SLIP_THRESHOLD)
                    slips++;
        }

        public void CountComplete(Vector pos, ROI roi, DateTime time, int count)
        {
            if (activeTaskStage == null)
                return;

            if (activeTaskStage.CheckSelection(roi.Name, activeSet))
            {
                // Update the time pointing metric
                IncrementTimePointing();

                // Update the selection time metric
                selectionTime = (long)DateTime.Now.Subtract((DateTime)enterWorkspace).TotalMilliseconds;

                NextTaskStage();
            }
            else
            {
                incorrectSelections++;
            }

            // If this selection starts a new activity, update the active set
            if (roi.ToActivate != null && roi.ToActivate.Length > 0)
                UpdateActiveSet(roi.ToActivate);
        }

        public void CountProgress(long dwellTime, ROI roi)
        {
            /*
             * If the hand is already in the workspace, we'll not have
             * set the enterWorkspace time. See if this needs set now to avoid error.
             */
            UpdateSelectionTime();
        }

        public void CursorUpdate(Vector pos, int count, int edge) {}
        public void GroupEnter(String name) {}
        public void GroupLeave(String name) {}

        public void Back()
        {
            // Don't include back gestures on the home screen as incorrect selections
            if (previousActiveSet == null)
                return;

            // Selecting the Back button in the Point interaction counts as a selection
            incorrectSelections++;

            // Go back to the previous ROI set
            UpdateActiveSet(previousActiveSet);
        }
        #endregion

        #region Util
        private void UpdateActiveSet(String newActiveSet)
        {
            if (newActiveSet == null)
                return;

            previousActiveSet = newActiveSet.Equals("Main") ? null : activeSet;
            activeSet = newActiveSet;
        }

        private void Log(String message)
        {
            Console.WriteLine("Experiment: {0},{1},{2}: {3}", participantNum, blockNum, Metadata.ConditionNames[conditionNum - 1], message);
        }

        private void LogExperimentData(String message)
        {
            Console.WriteLine(message);

            Logger.Write(String.Format("P{0}.csv", participantNum), message);
        }
        #endregion
    }
}
