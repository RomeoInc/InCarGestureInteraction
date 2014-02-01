using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Experiment
{
    public class Metadata
    {
        public enum FeedbackType { None, Constant, Amplitude, Roughness };

        public enum GestureType { Point, Count };

        public const long SLIP_THRESHOLD = 250;
        public const int N_CONDITIONS = 8;
        public const int TASKS_PER_BLOCK = 14;
        public const int PUREDATA_PORT = 34567;
        public const String INITIAL_SET = "Main";

        /// <summary>
        /// Names of conditions.
        /// </summary>
        public static String[] ConditionNames = new string[] {
            "Count-None", "Count-Constant",
            "Count-Amplitude", "Count-Roughness",
            "Point-None", "Point-Constant",
            "Point-Amplitude", "Point-Roughness"
        };

        /// <summary>
        /// Feedback given for each condition.
        /// </summary>
        public static FeedbackType[] ConditionFeedback = new FeedbackType[] {
            FeedbackType.None, FeedbackType.Constant,
            FeedbackType.Amplitude, FeedbackType.Roughness,
            FeedbackType.None, FeedbackType.Constant,
            FeedbackType.Amplitude, FeedbackType.Roughness
        };

        /// <summary>
        /// Gesture used for each condition.
        /// </summary>
        public static GestureType[] ConditionGesture = new GestureType[] {
            GestureType.Count, GestureType.Count,
            GestureType.Count, GestureType.Count,
            GestureType.Point, GestureType.Point,
            GestureType.Point, GestureType.Point
        };
    }
}
