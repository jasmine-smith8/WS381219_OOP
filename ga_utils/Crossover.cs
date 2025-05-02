namespace WS381219_OOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Crossover class for genetic algorithm
    // This class is responsible for performing crossover between two parent jobs
    // It creates a child job by combining tasks from both parents
    // The child job inherits the JobID from the parents
    // It ensures that tasks are not duplicated in the child job
    public class CrossoverImplementation
    {
        private CalculateFitnessImplementation fitnessCalculator = new CalculateFitnessImplementation();
        public Job Crossover(Job parent1, Job parent2)
        {
            // Create a child job with the same JobID as the parents
            var child = new Job(parent1.JobID);
            // Use a HashSet to track tasks already added to the child
            var addedTasks = new HashSet<(int JobID, int OperationID, int processingTime)>();

            // Add tasks from the first parent in order
            foreach (var task in parent1.Tasks)
            {
                if (!addedTasks.Contains((task.JobID, task.OperationID, task.ProcessingTime)))
                {
                    // Add the task to the child job
                    child.AddTask(task);
                    // Mark the task as added
                    addedTasks.Add((task.JobID, task.OperationID, task.ProcessingTime));
                }
            }
            // Add tasks from the second parent in order
            foreach (var task in parent2.Tasks)
            {
                if (!addedTasks.Contains((task.JobID, task.OperationID, task.ProcessingTime)))
                {
                    // Add the task to the child job
                    child.AddTask(task);
                    // Mark the task as added
                    addedTasks.Add((task.JobID, task.OperationID, task.ProcessingTime));
                }
            }
            child.Tasks = child.Tasks.OrderBy(t => t.OperationID).ToList(); // Sort tasks by OperationID
            
            fitnessCalculator.CalculateFitness(child); // Calculate fitness for the child job
            Console.WriteLine($"Crossover occurred. Added tasks: {addedTasks.Count}");
            // Return the child job
            return child;
        }
    }
}