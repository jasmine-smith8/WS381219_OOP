//WS381219-OOP
//Job Shop Scheduling Problem

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
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
            // Output file list
            Console.WriteLine("Select a job file:");
            foreach (string file in files)
            {
                Console.WriteLine(file);
            }
            // Prompt user to select a file
            Console.Write("Enter the file name (without extension): ");
            string fileName = Console.ReadLine() ?? string.Empty;
            // Check if the file exists
            string filePath = Path.Combine("jobs", fileName + ".csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return jobs;
            }
                // Read all lines from the CSV file
                string[] lines = File.ReadAllLines(filePath);
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
            // Export the schedule to a CSV file
            var scheduler = new Scheduler();
            foreach (var task in bestJob.Tasks)
            {
                scheduler.ScheduleTask(task);
            }
            var schedule = new ScheduleExporter(scheduler);
            schedule.ExportScheduleToXLSX("optimized_schedule.xlsx");
            // Print the schedule
            schedule.PrintSchedule();
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
    public class Scheduler
    {
        // Schedule maps StartTime to Task
        public SortedDictionary<Tuple<int, int>, Task> Schedule { get; set; }

        public Scheduler()
        {
            Schedule = new SortedDictionary<Tuple<int, int>, Task>();
        }

        // Method to add a task to the schedule
        public void ScheduleTask(Task task)
        {
            if (Schedule.Values.Any(existingTask => existingTask.StartTime == task.StartTime && existingTask.Subdivision == task.Subdivision))
            {
                Console.WriteLine($"Conflict detected: Task with StartTime {task.StartTime} already exists in subdivision {task.Subdivision}.");
            }
            else
            {
                Schedule[new Tuple<int, int>(task.StartTime, task.OperationID)] = task;
            }
        }
    }

    public class ScheduleExporter
    {
        private readonly Scheduler _scheduler;

        public ScheduleExporter(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void ExportScheduleToXLSX(string filePath)
        {
            // Check if the schedule is empty
            if (!_scheduler.Schedule.Any())
            {
                Console.WriteLine("The schedule is empty. No data to export.");
                return;
            }

            // Get all unique subdivision names (columns)
            var subdivisions = _scheduler.Schedule.Values.Select(task => task.Subdivision).Distinct().ToList();

            // Determine the time range (rows)
            int minStartTime = _scheduler.Schedule.Keys.Min(key => key.Item1);
            int maxStartTime = _scheduler.Schedule.Keys.Max(key => key.Item1);

            // Create a dictionary to map StartTime to a row in the XLS
            var timeRows = new Dictionary<int, Dictionary<string, string>>();

            // Initialize the rows
            for (int time = minStartTime; time <= maxStartTime; time++)
            {
                timeRows[time] = subdivisions.ToDictionary(subdivision => subdivision, _ => string.Empty);
            }

            // Populate the rows with task details
            foreach (var entry in _scheduler.Schedule)
            {
                int startTime = entry.Key.Item1;
                var task = entry.Value;

                // Add the task's details to the corresponding subdivision and start time
                timeRows[startTime][task.Subdivision] = $"JobID: {task.JobID} OpID: {task.OperationID}";
            }

            // Create an Excel file using library ClosedXML
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Schedule");

                // Write the header row
                worksheet.Cell(1, 1).Value = "StartTime";
                for (int i = 0; i < subdivisions.Count; i++)
                {
                    worksheet.Cell(1, i + 2).Value = subdivisions[i];
                }

                // Write each row
                int rowIndex = 2;
                foreach (var time in timeRows.Keys.OrderBy(t => t))
                {
                    // Convert the time (assumed to be in hours) to a formatted time string starting from 09:00
                    TimeSpan formattedTime = TimeSpan.FromHours(9) + TimeSpan.FromHours(time);
                    worksheet.Cell(rowIndex, 1).Value = formattedTime.ToString(@"hh\:mm"); // StartTime in HH:mm format

                    for (int i = 0; i < subdivisions.Count; i++)
                    {
                        worksheet.Cell(rowIndex, i + 2).Value = timeRows[time][subdivisions[i]];
                    }
                    rowIndex++;
                }

                // Save the workbook to the specified file path
                workbook.SaveAs(filePath);
            }

            Console.WriteLine($"Schedule exported to {filePath}");
        }

        public void PrintSchedule()
        {
            // Print the schedule in a readable format
            foreach (var entry in _scheduler.Schedule)
            {
                Console.WriteLine($"StartTime: {entry.Key}, Task: {entry.Value}");
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

                foreach (var job in jobs)
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
                    foreach (var job in jobs.OrderBy(_ => random.Next())) // Randomize job order
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
                int startTime = Math.Max(machineTimes[machineID], makespan);
                // Update the end time based on the processing time
                int endTime = startTime + processingTime;
                // Update the makespan to the maximum of the current makespan and the end time
                makespan = endTime;
                // Update the machine time for the current machine ID
                machineTimes[machineID] = startTime + processingTime;
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
            // Create a child job with the same JobID as the parents
            var child = new Job(parent1.JobID);

            // Use a HashSet to track tasks already added to the child
            var addedTasks = new HashSet<(int JobID, int OperationID)>();

            // Add tasks from the first parent in order
            foreach (var task in parent1.Tasks)
            {
                if (!addedTasks.Contains((task.JobID, task.OperationID)))
                {
                    child.AddTask(task);
                    addedTasks.Add((task.JobID, task.OperationID));
                }
            }

            // Add tasks from the second parent in order, ensuring no duplicates
            foreach (var task in parent2.Tasks)
            {
                if (!addedTasks.Contains((task.JobID, task.OperationID)))
                {
                    child.AddTask(task);
                    addedTasks.Add((task.JobID, task.OperationID));
                }
            }

            // Calculate fitness for the child
            CalculateFitness(child);

            // Return the child job
            return child;
        }


        private void Mutate(Job child)
        {
            // Randomly select two tasks to swap within the same job
            int index1 = random.Next(child.Tasks.Count);
            int index2 = random.Next(child.Tasks.Count);

            // Ensure the tasks belong to the same job
            if (child.Tasks[index1].JobID == child.Tasks[index2].JobID)
            {
                // Swap the tasks
                var temp = child.Tasks[index1];
                child.Tasks[index1] = child.Tasks[index2];
                child.Tasks[index2] = temp;

                // Recalculate fitness after mutation
                CalculateFitness(child);
                Console.WriteLine("Mutation occurred.");
            }
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