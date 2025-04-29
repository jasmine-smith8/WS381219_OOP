//WS381219-OOP
//Job Shop Scheduling Problem
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
            // Read user input for the file name and allow empty input
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
            // Initialise stop watch
            var stopWatch = new Stopwatch();
            stopWatch.Start();
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

            //Output the result
            Console.WriteLine($"Best job fitness: {bestJob.Fitness}");
            // Export the schedule to a CSV file
            var scheduler = new Scheduler();
            foreach (var task in bestJob.Tasks)
            {
                scheduler.ScheduleTask(task);
            }
            var schedule = new ScheduleExporter(scheduler);
            // Stop the stopwatch
            stopWatch.Stop();
            // Allow user to name file
            Console.Write("Enter the name of the output file (without extension): ");
            var name = Console.ReadLine();
            // Export the schedule to an XLSX file
            schedule.ExportScheduleToXLSX($"{name}.xlsx");
            // Print the schedule
            schedule.PrintSchedule();
            // Output the elapsed time
            Console.WriteLine($"Elapsed time: {stopWatch.ElapsedMilliseconds} ms");
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
        // SortedDictionary is used to maintain the order of tasks based on their start time
        // Tuple<int, int> is used to represent the key (StartTime, OperationID)
        // Task is the value associated with the key
        public SortedDictionary<Tuple<int, int>, Task> Schedule { get; set; }
        // Dictionary to hold total makespan for overall schedule
        // Dictionary<int, int> is used to map machine ID to its makespan
        public Dictionary<int, int> Makespan { get; set; }

        public Scheduler()
        {
            Schedule = new SortedDictionary<Tuple<int, int>, Task>();
            Makespan = new Dictionary<int, int>();
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
            var timeRows = new Dictionary<int, Dictionary<string, Task>>(); // Use Task instead of string

            // Initialize the rows
            for (int time = minStartTime; time <= maxStartTime; time++)
            {
                timeRows[time] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task)null); // Allow null for Task
            }

            // Populate the rows with task details
            foreach (var entry in _scheduler.Schedule)
            {
                int startTime = entry.Key.Item1;
                var task = entry.Value;

                // Ensure job order is respected
                var previousTask = _scheduler.Schedule.Values
                    .Where(t => t.JobID == task.JobID && t.OperationID < task.OperationID)
                    .OrderByDescending(t => t.OperationID)
                    .FirstOrDefault();

                if (previousTask != null && previousTask.EndTime > startTime)
                {
                    // Adjust the start time to respect job order
                    startTime = previousTask.EndTime;
                }

                // Dynamically expand timeRows if startTime exceeds the current maxStartTime
                while (startTime > maxStartTime)
                {
                    maxStartTime++;
                    timeRows[maxStartTime] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task)null);
                }

                // Add the task to the corresponding subdivision and start time
                timeRows[startTime][task.Subdivision] = task;
            }
            // Final check to ensure all rows are included
            int maxEncounteredStartTime = _scheduler.Schedule.Values.Max(t => t.EndTime);
            while (maxEncounteredStartTime > maxStartTime)
            {
                maxStartTime++;
                timeRows[maxStartTime] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task)null);
            }
            // Generate a color mapping for Job IDs
            var jobIdColors = GenerateJobIdColors();
            // Create an Excel file using library ClosedXML
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Schedule");

                // Write the header row
                worksheet.Cell(1, 1).Value = "StartTime";
                worksheet.Cell(1, 1).Style.Font.Bold = true; // Make the header bold
                for (int i = 0; i < subdivisions.Count; i++)
                {
                    worksheet.Cell(1, i + 2).Value = subdivisions[i];
                    worksheet.Cell(1, i + 2).Style.Font.Bold = true; // Make the header bold
                }
                // Write each row
                int rowIndex = 2;
                foreach (var time in timeRows.Keys.OrderBy(t => t))
                {
                    // Convert the time (assumed to be in hours) to a formatted time range starting from 09:00
                    TimeSpan startTime = TimeSpan.FromHours(9) + TimeSpan.FromHours(time);
                    TimeSpan endTime = startTime + TimeSpan.FromHours(1); // Add 1 hour for the end time
                    worksheet.Cell(rowIndex, 1).Value = $"{startTime:hh\\:mm}-{endTime:hh\\:mm}"; // Format as StartTime-EndTime

                    for (int i = 0; i < subdivisions.Count; i++)
                    {
                        var task = timeRows[time][subdivisions[i]];
                        if (task != null)
                        {
                            // Calculate the range of rows to merge based on ProcessingTime
                            int startRow = rowIndex;
                            int endRow = startRow + task.ProcessingTime - 1;

                            // Merge the cells for the task's duration
                            worksheet.Range(startRow, i + 2, endRow, i + 2).Merge();

                            // Write the task details in the first cell of the merged range
                            worksheet.Cell(startRow, i + 2).Value = $"JobID: {task.JobID} OpID: {task.OperationID}";

                            // Apply color based on JobID
                            worksheet.Range(startRow, i + 2, endRow, i + 2).Style.Fill.BackgroundColor = jobIdColors[task.JobID];
                        }
                    }
                    rowIndex++;
                }

                // Display the total makespan which is equal to the last end time in the schedule  
                worksheet.Cell(rowIndex, 1).Value = "Total Makespan";
                worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;
                worksheet.Cell(rowIndex, 2).Value = $"{_scheduler.Schedule.Values.Max(t => t.EndTime)}";
                rowIndex++;
                // Add a row for the end of the schedule
                worksheet.Range(rowIndex, 1, rowIndex, subdivisions.Count + 1).Merge();
                worksheet.Cell(rowIndex, 1).Value = "End of Schedule";
                worksheet.Cell(rowIndex, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;
                worksheet.Cell(rowIndex, 1).Style.Fill.BackgroundColor = XLColor.Red;
                worksheet.Cell(rowIndex, 1).Style.Font.FontColor = XLColor.White;
                // Display average number of tasks per job
                worksheet.Cell(rowIndex + 1, 1).Value = "Average Tasks per Job";
                worksheet.Cell(rowIndex + 1, 1).Style.Font.Bold = true;
                worksheet.Cell(rowIndex + 1, 2).Value = $"{_scheduler.Schedule.Values.GroupBy(t => t.JobID).Average(g => g.Count())}";
                // Display the number of jobs
                worksheet.Cell(rowIndex + 2, 1).Value = "Number of Jobs";
                worksheet.Cell(rowIndex + 2, 1).Style.Font.Bold = true;
                worksheet.Cell(rowIndex + 2, 2).Value = $"{_scheduler.Schedule.Values.Select(t => t.JobID).Distinct().Count()}";
                // Display the number of subdivisions
                worksheet.Cell(rowIndex + 3, 1).Value = "Number of Subdivisions";
                worksheet.Cell(rowIndex + 3, 1).Style.Font.Bold = true;
                worksheet.Cell(rowIndex + 3, 2).Value = $"{subdivisions.Count}";
                // Display the number of tasks
                worksheet.Cell(rowIndex + 4, 1).Value = "Total Number of Tasks";
                worksheet.Cell(rowIndex + 4, 1).Style.Font.Bold = true;
                worksheet.Cell(rowIndex + 4, 2).Value = $"{_scheduler.Schedule.Values.Count}";
                // Save the workbook to the specified file path
                workbook.SaveAs(filePath);
            }

            Console.WriteLine($"Schedule exported to {filePath}");
        }

        // Helper method to generate a color mapping for Job IDs
        private Dictionary<int, XLColor> GenerateJobIdColors()
        {
            var colors = new List<XLColor>
            {
                XLColor.LightBlue,
                XLColor.Aquamarine,
                XLColor.LightYellow,
                XLColor.PaleTurquoise,
                XLColor.LightPink,
                XLColor.MayaBlue,
                XLColor.BubbleGum,
                XLColor.LavenderPink,
                XLColor.BabyBlueEyes,
                XLColor.LightPastelPurple,
            };

            var jobIdColors = new Dictionary<int, XLColor>();
            int colorIndex = 0;

            foreach (var jobId in _scheduler.Schedule.Values.Select(task => task.JobID).Distinct())
            {
                jobIdColors[jobId] = colors[colorIndex % colors.Count];
                colorIndex++;
            }

            return jobIdColors;
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
        // This is a Fisher-Yates shuffle algorithm to randomize the order of jobs
        // This method takes a list and a random number generator as input
        // It shuffles the list in place to create a random order
        public static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]); // Swap elements
            }
        }
        public List<Job> InitializePopulation()
        {
            // Initialize the population with jobs
            var population = new List<Job>();

            for (int i = 0; i < populationSize; i++)
            {
                var machineAvailability = new Dictionary<int, int>();
                var jobProgress = new Dictionary<int, int>();
                var schedule = new List<Task>();

                var shuffledJobs = jobs.ToList(); // Create a copy of the jobs list
                Shuffle(shuffledJobs, this.random); // Shuffle the jobs to randomize the order

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
            // Create a dictionary to track the availability of each machine
            var machineAvailability = new Dictionary<int, int>();
            // Initialize makespan to 0
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
            Console.WriteLine($"Schedule Generation fitness: {individual.Fitness}");
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
            
            CalculateFitness(child); // Calculate fitness for the child job
            Console.WriteLine($"Crossover occurred. Added tasks: {addedTasks.Count}");
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