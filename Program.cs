//WS381219-OOP
//Job Shop Scheduling Problem

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// namespace is used to organize code and avoid naming conflicts
namespace WS381219_OOP
{
    public class Program
    {
        // Nested dictionary to hold job data so that each job can have multiple subdivisions and their respective processing times
        public static Dictionary<int, List<(int OperationID, string Subdivision, int ProcessingTime)>> LoadJobsFromCSV()
        {
            // Create a dictionary to hold job data
            var jobs = new Dictionary<int, List<(int OperationID, string Subdivision, int ProcessingTime)>>();
            // Get all CSV files in the "jobs" directory
            string[] files = Directory.GetFiles("jobs", "*.csv");
            // Iterate through each file
            foreach (string file in files)
            {
                // Read all lines from the CSV file
                string[] lines = File.ReadAllLines(file);
                // Skip the header line and process each subsequent line
                for (int i = 1; i < lines.Length; i++)
                {
                    // Split the line by commas to get job details
                    string line = lines[i];
                    string[] cells = line.Split(',');
                    // Check if the line has the expected number of columns
                    if (cells.Length == 4)
                    {
                        // Parse the job ID, operation ID, subdivision, and processing time
                        int jobID = int.Parse(cells[0].Trim());
                        int OperationID = int.Parse(cells[1].Trim());
                        string Subdivision = cells[2].Trim();
                        int ProcessingTime = int.Parse(cells[3].Trim());

                        // Check if the job already exists
                        if (!jobs.ContainsKey(jobID))
                        {
                            // If not, create a new entry for the job
                            jobs[jobID] = new List<(int OperationID, string Subdivision, int ProcessingTime)>();
                        }

                        // Add or update the subdivision and time for the job
                        jobs[jobID].Add((OperationID, Subdivision, ProcessingTime));
                    }
                }
            }
            // Return the populated dictionary of jobs
            return jobs;
        }

        public static void Main(string[] args)
        {
            // Load jobs from CSV files
            var jobs = LoadJobsFromCSV();
            // Set mutation and crossover parameters  
            int populationSize = 10;
            double mutationRate = 0.1;
            int maxGenerations = 100;
            int tournamentSize = 3;
            // Set machine and time parameters
            var machine = new Dictionary<int, int>();
            var time = new Dictionary<int, int>();

            // Create GAJSS instance
            var ga = new GAJSS(jobs.Select(jobEntry => 
            {
                // Create a new job instance for each job entry
                var job = new Job(jobEntry.Key); 
                foreach (var task in jobEntry.Value) 
                {
                    // Add tasks to the job based on the subdivisions and processing times
                    job.AddTask(new Task(jobEntry.Key, task.OperationID, task.Subdivision, task.ProcessingTime));
                }
                // Return the job instance
                return job;
            }).ToList(), machine, time, populationSize, mutationRate, maxGenerations, tournamentSize);

            // Solve the problem
            var bestJob = ga.Solve();

            // Output the optimized schedule
            Console.WriteLine("Optimized Schedule:");
            var groupedTasks = bestJob.Tasks.GroupBy(task => task.JobID);

            foreach (var group in groupedTasks)
            {
                Console.WriteLine($"Job ID: {group.Key}");
                foreach (var task in group)
                {
                    Console.WriteLine($"  OperationID: {task.OperationID}, Subdivision: {task.Subdivision}, ProcessingTime: {task.ProcessingTime}, StartTime: {task.StartTime}, EndTime: {task.EndTime}");
                }
            }

            // Output the result
            Console.WriteLine($"Best job fitness: {bestJob.Fitness}");
        }
    }

    public class Job
    {
        // jobID represents the unique identifier for each job
        public int JobID { get; set; }
        // Tasks is a list of tasks associated with the job
        public List<Task> Tasks { get; set; }
        // Fitness is used to evaluate the quality of the job schedule
        public int Fitness { get; set; }
        // Stack to store jobs in correct task order
        private Stack<Task> jobStack;

        // Constructor to initialize a job with a unique ID
        public Job(int jobID)
        {
            JobID = jobID;
            Tasks = new List<Task>();
            // Initialize the jobStack here
            jobStack = new Stack<Task>(Tasks.Where(task => task.JobID == JobID));
        }
        // Method to add a task to the job
        public void AddTask(Task task)
        {
            // Add the task to the list of tasks
            Tasks.Add(task);
            Console.WriteLine($"Task added: {task}");
        }
    }

    public class Task
    {
        // Properties of the Task class
        public int JobID { get; set; }           // ID of the job
        public int OperationID { get; set; }        // ID of the machine
        public string Subdivision { get; set; }     // Machine name or label
        public int ProcessingTime { get; set; }     // Time the task takes
        public int StartTime { get; set; }          // When the task starts
        public int EndTime => StartTime + ProcessingTime; // Calculated automatically

        public Task(int jobID, int operationID, string subdivision, int processingTime)
        {
            JobID = jobID;
            OperationID = operationID;
            Subdivision = subdivision;
            ProcessingTime = processingTime;
        }

        public override string ToString()
        {
            return $"[JobID: {JobID}, Subdivision: {Subdivision}] Start: {StartTime}, End: {EndTime}, Duration: {ProcessingTime}";
        }
    }

    // Scheduling class
    class Scheduler
    {
        // Method to schedule tasks based on their start and end times
        public static void ScheduleTasks(List<Task> tasks)
        {
            // Sort tasks by their start time
            tasks.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            // Output the scheduled tasks
            foreach (var task in tasks)
            {
                Console.WriteLine(task);
            }
        }
    }


    class GAJSS
    {
        private Random random = new Random();
        private List<Job> jobs;
        private Dictionary<int, int> machine;
        private Dictionary<int, int> time;
        private int populationSize;
        private double mutationRate;
        private int maxGenerations;
        private int tournementSize;

        public GAJSS(List<Job> jobs, Dictionary<int, int> machine, Dictionary<int, int> time, int populationSize, double mutationRate, int maxGenerations, int tournementSize)
        {
            // Initialize the genetic algorithm with the provided parameters
            this.jobs = jobs;
            this.machine = machine;
            this.time = time;
            this.populationSize = populationSize;
            this.mutationRate = mutationRate;
            this.maxGenerations = maxGenerations;
            this.tournementSize = tournementSize;
        }

        private List<Job> InitializePopulation()
        {
            var population = new List<Job>();

            for (int i = 0; i < populationSize; i++)
            {
                var machineAvailability = new Dictionary<int, int>();
                var jobProgress = new Dictionary<int, int>();
                var schedule = new List<Task>();

                bool allDone;
                do
                {
                    allDone = true;
                    foreach (var job in jobs.OrderBy(_ => random.Next())) // randomize job order
                    {
                        int jobID = job.JobID;
                        if (!jobProgress.ContainsKey(jobID)) jobProgress[jobID] = 0;

                        int nextTaskIndex = jobProgress[jobID];
                        if (nextTaskIndex >= job.Tasks.Count)
                            continue;

                        allDone = false;

                        var task = job.Tasks[nextTaskIndex];
                        int machineID = task.OperationID;

                        if (!machineAvailability.ContainsKey(machineID))
                            machineAvailability[machineID] = 0;

                        int earliestStart = 0;
                        // Respect job order
                        if (nextTaskIndex > 0)
                        {
                            var prevTask = job.Tasks[nextTaskIndex - 1];
                            earliestStart = schedule
                                .Where(t => t.OperationID == prevTask.OperationID && t.Subdivision == prevTask.Subdivision)
                                .Select(t => t.EndTime) // Use EndTime to respect job order
                                .Max();
                        }

                        int start = Math.Max(earliestStart, machineAvailability[machineID]);
                        machineAvailability[machineID] = start + task.ProcessingTime;

                        // Add task with calculated start time
                        schedule.Add(new Task(task.JobID, task.OperationID, task.Subdivision, task.ProcessingTime));
                        schedule.Last().StartTime = start;
                        jobProgress[jobID]++;
                    }
                } while (!allDone);

                var newJob = new Job(i);
                foreach (var t in schedule)
                    newJob.AddTask(t);

                CalculateFitness(newJob);
                population.Add(newJob);
            }

            return population;
        }


        private void CalculateFitness(Job individual)
        {
            // Create a dictionary to hold the machine times
            var machineTimes = new Dictionary<int, int>();
            // Initialize makespan to 0
            int makespan = 0;
            // Iterate through each task in the job
            foreach (var task in individual.Tasks)
            {
                // Get the machine ID and processing time for the task
                int machineID = task.OperationID;
                int processingTime = task.ProcessingTime;
                // Check if the machine ID exists in the dictionary
                if (!machineTimes.ContainsKey(machineID))
                {   // If not, initialize it to 0    
                    machineTimes[machineID] = 0;
                }
                // Calculate the start and end times for the task
                int startTime = Math.Max(makespan, machineTimes[machineID]);
                // Update the end time based on the processing time
                int endTime = startTime + processingTime;
                // Update the makespan to the maximum of the current makespan and the end time
                makespan = endTime;
                // Update the machine time for the current machine ID
                machineTimes[machineID] = endTime;
            }
            // Set the fitness of the individual job to the makespan
            individual.Fitness = makespan;
        }

        private Job TournamentSelection(List<Job> population)
        {
            // Select a random subset of the population for tournament selection
            var tournament = new List<Job>();
            // Iterate through the tournament size
            for (int i = 0; i < tournementSize; i++)
            {
                // Select a random job from the population
                int index = random.Next(population.Count);
                // Add the selected job to the tournament
                tournament.Add(population[index]);
            }
            // Calculate fitness for each job in the tournament
            tournament.ForEach(CalculateFitness);
            // Select the job with the best fitness (minimum makespan)
            return tournament.OrderBy(job => job.Fitness).First();
        }

        private Job Crossover(Job parent1, Job parent2)
        {
            // Perform crossover between two parent jobs to create a child job
            var child = new Job(parent1.JobID);
            // Randomly select a crossover point
            var crossoverPoint = random.Next(1, Math.Min(parent1.Tasks.Count, parent2.Tasks.Count));
            // Iterate through the tasks of the first parent up to the crossover point
            for (int i = 0; i < crossoverPoint; i++)
            {
                // Add the task from the first parent to the child
                child.AddTask(parent1.Tasks[i]);
            }
            // Iterate through the tasks of the second parent from the crossover point
            for (int i = crossoverPoint; i < parent2.Tasks.Count; i++)
            {
                // Add the task from the second parent to the child
                child.AddTask(parent2.Tasks[i]);
            }
            // Calculate fitness for the child job
            CalculateFitness(child);
            // Return the child job
            return child;
        }

        private void Mutate(Job child)
        {
            // Randomly select two tasks to swap
            int index1 = random.Next(child.Tasks.Count);
            int index2 = random.Next(child.Tasks.Count);
            // Create a temporary variable to swap the tasks
            var temp = child.Tasks[index1];
            // Swap the tasks
            child.Tasks[index1] = child.Tasks[index2];
            child.Tasks[index2] = temp;
            // Recalculate fitness after mutation
            CalculateFitness(child);

            Console.WriteLine("Mutation occurred.");
        }

        public Job Solve()
        {
            // Initialize the population
            var population = InitializePopulation();
            // Loop through generations
            for (int generation = 0; generation < maxGenerations; generation++)
            {
                // Create a new population
                var newPopulation = new List<Job>();
                // Iterate through the population
                for (int i = 0; i < populationSize; i++)
                {
                    // Select parents using tournament selection
                    var parent1 = TournamentSelection(population);
                    var parent2 = TournamentSelection(population);
                    // Perform crossover and mutation 
                    var child = Crossover(parent1, parent2);
                    // If the mutation rate is met, mutate the child
                    if (random.NextDouble() < mutationRate)
                    {
                        Mutate(child);
                    }
                    // Calculate fitness for the child
                    CalculateFitness(child);
                    // Add the child to the new population
                    newPopulation.Add(child);
                }
                // Replace the old population with the new one
                population = newPopulation;
                // Output the number of generations completed
                Console.WriteLine($"Generation {generation + 1} completed.");
            }
            // Return the best job from the final population
            return population.OrderBy(job => job.Fitness).First();
        }
    }
}