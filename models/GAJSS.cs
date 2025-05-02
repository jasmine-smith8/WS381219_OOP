namespace WS381219_OOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Genetic Algorithm for Job Shop Scheduling (GAJSS)
    // This class implements a genetic algorithm to solve the job shop scheduling problem
    // It uses various components like fitness calculation, crossover, mutation, and selection
    // The goal is to find an optimal or near-optimal schedule for jobs on machines

    public class GAJSS
    {
        private Random random = new Random();
        private List<Job> jobs;
        private Dictionary<int, int> machine;
        private Dictionary<int, int> time;
        private int populationSize;
        private double mutationRate;
        private int maxGenerations;
        private int tournementSize;

        private CalculateFitnessImplementation fitnessCalculator = new CalculateFitnessImplementation();
        private CrossoverImplementation crossover = new CrossoverImplementation();
        private MutateChild mutate = new MutateChild();
        private Selection tournamentSelection = new Selection();
        private GAUtils gaUtils = new GAUtils();

        public GAJSS(List<Job> jobs, Dictionary<int, int> machine, Dictionary<int, int> time, int populationSize, double mutationRate, int maxGenerations, int tournementSize)
        {
            // Initialise the genetic algorithm with the provided parameters
            this.jobs = jobs;
            this.machine = machine;
            this.time = time;
            this.populationSize = populationSize;
            this.mutationRate = mutationRate;
            this.maxGenerations = maxGenerations;
            this.tournementSize = tournementSize;
        }
        public List<Job> InitialisePopulation()
        {
            // Initialise the population with jobs
            var population = new List<Job>();

            for (int i = 0; i < populationSize; i++)
            {
                var machineAvailability = new Dictionary<int, int>();
                var jobProgress = new Dictionary<int, int>();
                var schedule = new List<Task>();

                var shuffledJobs = jobs.ToList(); // Create a copy of the jobs list
                GAUtils.Shuffle(shuffledJobs, this.random); // Shuffle the jobs to randomise the order

                foreach (var job in shuffledJobs)
                {
                    int jobID = job.JobID;
                    int previousTaskEndTime = 0; // Tracks the end time of the previous task in the job

                    foreach (var task in job.Tasks)
                    {
                        int machineID = task.OperationID;

                        // Ensure the machine has an entry in the availability dictionary
                        if (!machineAvailability.ContainsKey(machineID))
                            machineAvailability[machineID] = 0;

                        // Calculate the earliest start time for the task
                        int earliestStart = Math.Max(previousTaskEndTime, machineAvailability[machineID]);

                        // Check for conflicts
                        bool conflictDetected = schedule.Any(existingTask =>
                            existingTask.Subdivision == task.Subdivision &&
                            existingTask.StartTime < earliestStart + task.ProcessingTime &&
                            earliestStart < existingTask.EndTime);

                        if (conflictDetected)
                        {
                            // Adjust the start time to resolve the conflict
                            earliestStart = schedule
                                .Where(existingTask => existingTask.Subdivision == task.Subdivision)
                                .Max(existingTask => existingTask.EndTime);
                        }

                        // Schedule the task
                        var scheduledTask = new Task(task.JobID, task.OperationID, task.Subdivision, task.ProcessingTime)
                        {
                            StartTime = earliestStart
                        };

                        // Update machine availability and previous task end time
                        machineAvailability[machineID] = scheduledTask.EndTime;
                        previousTaskEndTime = scheduledTask.EndTime;

                        // Add the task to the schedule
                        schedule.Add(scheduledTask);
                    }
                }

                bool allDone;
                do
                {
                    allDone = true;
                    foreach (var job in shuffledJobs) 
                    {
                        // Check if all tasks in the job are done
                        int jobID = job.JobID;
                        if (!jobProgress.ContainsKey(jobID)) jobProgress[jobID] = 0;

                        // If all tasks are done, continue to the next job
                        int nextTaskIndex = jobProgress[jobID];
                        if (nextTaskIndex >= job.Tasks.Count)
                            continue;
                        // If not all tasks are done, set allDone to false
                        allDone = false;

                        // Get the next task to schedule
                        var task = job.Tasks[nextTaskIndex];
                        int machineID = task.OperationID;

                        // Ensure the machine has an entry in the availability dictionary
                        if (!machineAvailability.ContainsKey(machineID))
                            machineAvailability[machineID] = 0;

                        int earliestStart = 0;
                        // Respect job order
                        if (nextTaskIndex > 0)
                        {
                            var prevTask = job.Tasks[nextTaskIndex - 1];
                            earliestStart = schedule
                                .Where(t => t.JobID == prevTask.JobID)
                                .Select(t => t.EndTime)
                                .Max();
                        }
                        
                        int start = Math.Max(earliestStart, machineAvailability[machineID]);
                        machineAvailability[machineID] = start + task.ProcessingTime;

                        // Add task with calculated start time
                        var scheduledTask = new Task(task.JobID, task.OperationID, task.Subdivision, task.ProcessingTime)
                        {
                            StartTime = start
                        };
                        schedule.Add(scheduledTask);
                        jobProgress[jobID]++;
                    }
                } while (!allDone);

                var newJob = new Job(i);
                foreach (var t in schedule)
                    newJob.AddTask(t);
                // Calculate fitness for the new job
                fitnessCalculator.CalculateFitness(newJob);
                population.Add(newJob);
            }

            return population;
        }
        public Job Solve()
        {
            var population = InitialisePopulation();
            double bestMakespan = double.MaxValue;
            Job bestJob = tournamentSelection.GetBestJob(population);

            // This counter is used to track stagnation, and it is reset when a better solution is found
            int stagnationCounter = 0;
            int stagnationLimit = 10;

            // Main loop for the genetic algorithm
            for (int generation = 0; generation < maxGenerations; generation++)
            {
                var newPopulation = new List<Job>();
                //for each individual in the population, perform selection, crossover, and mutation
                foreach (var _ in Enumerable.Range(0, populationSize))
                {
                    var parent1 = tournamentSelection.TournamentSelection(population, tournementSize, random);
                    var parent2 = tournamentSelection.TournamentSelection(population, tournementSize, random);
                    var child = crossover.Crossover(parent1, parent2);

                    if (random.NextDouble() < mutationRate)
                    {
                        mutate.Mutate(child);
                    }

                    // Calculate fitness for the child
                    fitnessCalculator.CalculateFitness(child);

                    // Validate the child solution
                    if (gaUtils.IsValidSolution(child))
                    {
                        // If the child's makespan is better than the current best, update the best job
                        if (child.Fitness < bestMakespan)
                        {
                            bestMakespan = child.Fitness;
                            bestJob = child;
                            stagnationCounter = 0;
                        }

                        // Add the child to the new population
                        newPopulation.Add(child);
                    }
                    else
                    {
                        var validJob = population[random.Next(population.Count)];
                        mutate.Mutate(validJob);
                        // Recalculate fitness for the valid job
                        fitnessCalculator.CalculateFitness(validJob);
                        newPopulation.Add(validJob);
                    }
                }

                // Replace the old population with the new one
                population = newPopulation;

                // Output the current generation and best makespan
                Console.WriteLine($"Generation {generation + 1}: Best Makespan = {bestMakespan}");
                // Increment stagnation counter if no improvement
                if (population.All(job => job.Fitness == bestJob.Fitness))
                {
                    stagnationCounter++;
                    if (stagnationCounter >= stagnationLimit)
                    {
                        Console.WriteLine("Terminating early due to stagnation.");
                        break;
                    }
                }  
            }
            // Return the best job found
            return bestJob;
        }
    }
}
