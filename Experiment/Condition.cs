using System;
using System.Collections.Generic;
using Experiment.Tasks;

namespace Experiment
{
    public class Condition
    {
        private String name;
        private Task[] tasks;

        public Condition(String name, List<Task> tasks)
        {
            this.name = name;

            Tasks = tasks.ToArray();
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public Task this[int i]
        {
            get { return tasks[i]; }
        }

        public Task[] Tasks
        {
            get { return tasks; }
            set { tasks = Shuffle.RandomPermutation<Task>(value); } // Randomise order
        }

        public int TaskCount
        {
            get { return tasks.Length; }
        }
    }
}
