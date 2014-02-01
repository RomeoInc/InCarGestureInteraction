using Leap;

namespace Leap
{
    public class LeapInterface
    {
        private Controller controller;
        private LeapListener listener;

        public LeapInterface()
        {
            listener = new LeapListener();
            controller = new Controller();
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);

            Start();
        }

        #region Frames
        public void RegisterFrameListener(IFrameListener frameListener)
        {
            listener.RegisterFrameListener(frameListener);
        }

        public void UnregisterFrameListener(IFrameListener frameListener)
        {
            listener.UnregisterFrameListener(frameListener);
        }
        #endregion

        #region Control
        public void Start()
        {
            // Listen for updates from the Leap controller
            controller.AddListener(listener);
        }

        public void Stop()
        {
            // Stop listening for updates
            controller.RemoveListener(listener);
        }

        public void Destroy()
        {
            controller.Dispose();
        }
        #endregion
    }
}
