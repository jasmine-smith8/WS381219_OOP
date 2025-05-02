namespace WS381219_OOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GAUtils
    {
        // This is a Fisher-Yates shuffle algorithm to randomise the order of jobs
        // This method takes a list and a random number generator as input
        // It shuffles the list in place to create a random order
        // This is a common algorithm used in genetic algorithms to create diversity in the population
        public static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]); // Swap elements
            }
        }

        // Method to validate a solution
        public bool IsValidSolution(Job job)
        {
            // Ensure tasks respect order constraints
            foreach (var task in job.Tasks)
            {
                var previousTask = job.Tasks
                    .Where(t => t.JobID == task.JobID && t.OperationID < task.OperationID)
                    .OrderByDescending(t => t.OperationID)
                    .FirstOrDefault();

                if (previousTask != null && previousTask.EndTime > task.StartTime)
                {
                    return false; // Task order violated
                }
            }

            // Ensure no conflicts in the same subdivision
            foreach (var task in job.Tasks)
            {
                var conflictingTask = job.Tasks
                    .Where(t => t.Subdivision == task.Subdivision && t != task)
                    .FirstOrDefault(t => t.StartTime < task.EndTime && task.StartTime < t.EndTime);

                if (conflictingTask != null)
                {
                    return false; // Subdivision conflict detected
                }
            }

            return true; // Solution is valid
        }
    }
}