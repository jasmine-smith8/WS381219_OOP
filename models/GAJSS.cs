namespace WS381219_OOP
{
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
        // This is a common algorithm used in genetic algorithms to create diversity in the population
        public static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]); // Swap elements
            }
        }
        public List<Job> InitialisePopulation()
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
            var population = InitialisePopulation();
            
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