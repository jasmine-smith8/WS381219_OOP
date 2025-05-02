namespace WS381219_OOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CalculateFitnessImplementation
    {
        // This method calculates the fitness of a job based on its makespan
        // The makespan is the total time required to complete all tasks in the job
        // It takes into account the availability of machines and the order of tasks
        // It also detects conflicts between tasks in the same subdivision
        // The fitness is set to the makespan value
        public void CalculateFitness(Job individual)
        {
            // Create a dictionary to track the availability of each machine
            var machineAvailability = new Dictionary<int, int>();
            // Initialise makespan to 0
            int makespan = 0;

            // Create a list to track scheduled tasks for conflict detection
            var scheduledTasks = new List<Task>();

            // Iterate through each task in the job
            foreach (var task in individual.Tasks)
            {
                // Get the machine ID and processing time for the task
                int machineID = task.OperationID;
                int processingTime = task.ProcessingTime;

                // Ensure the machine has an entry in the availability dictionary
                if (!machineAvailability.ContainsKey(machineID))
                {
                    machineAvailability[machineID] = 0;
                }

                // Calculate the earliest start time for the task
                int earliestStart = machineAvailability[machineID];

                // Respect job order
                var previousTask = scheduledTasks
                    .Where(t => t.JobID == task.JobID && t.OperationID < task.OperationID)
                    .OrderByDescending(t => t.OperationID)
                    .FirstOrDefault();

                if (previousTask != null)
                {
                    earliestStart = Math.Max(earliestStart, previousTask.EndTime);
                }

                // Resolve conflicts with tasks in the same subdivision
                bool conflictDetected;
                do
                {
                    conflictDetected = false;

                    foreach (var existingTask in scheduledTasks)
                    {
                        if (existingTask.Subdivision == task.Subdivision &&
                            existingTask.StartTime < earliestStart + processingTime &&
                            earliestStart < existingTask.EndTime)
                        {
                            // Adjust the start time to resolve the conflict
                            earliestStart = existingTask.EndTime;
                            conflictDetected = true;
                        }
                    }
                } while (conflictDetected);

                // Update the task's start time
                task.StartTime = earliestStart;

                // Update the machine's availability and the makespan
                machineAvailability[machineID] = task.EndTime;
                makespan = Math.Max(makespan, task.EndTime);

                // Add the task to the list of scheduled tasks
                scheduledTasks.Add(task);
            }

            // Set the fitness of the individual job to the makespan
            individual.Fitness = makespan;
            Console.WriteLine($"Task fitness: {individual.Fitness}");
        }
    }       
}