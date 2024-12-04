
//WS381219-OOP
//Job Shop Scheduling Problem
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

public class Job
{
    public int JobID { get; set; }
    public List<Task> Tasks { get; private set; }

    public Job(int jobID)
    {
        JobID = jobID;
        Tasks = new List<Task>();
    }

    public static void LoadJobsFromCSV()
    {
        var jobs = new Dictionary<int, Job>();
        string selectedJob;

        try
        {
            Console.WriteLine("Select the jobset from the list: ");
            Console.WriteLine("1: example_job.csv");
            selectedJob = Console.ReadLine();

            string[] lines = File.ReadAllLines($"jobs/{selectedJob}");

            // Skip the header row
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] cells = line.Split(',');

                if (cells.Length == 4)
                {
                    // Parse jobID and taskID
                    int jobID = int.Parse(cells[0].Trim());
                    int taskID = int.Parse(cells[1].Trim());
                    string subdivision = cells[2].Trim();
                    int time = int.Parse(cells[3].Trim());

                    // Add or retrieve the job
                    if (!jobs.ContainsKey(jobID))
                    {
                        jobs[jobID] = new Job(jobID);
                    }

                    // Create the task
                    Task task = new Task(jobID, taskID, subdivision, time);

                    // Assign dependencies
                    task.AssignTaskDependencies(jobs[jobID].Tasks);

                    // Add task to job
                    jobs[jobID].Tasks.Add(task);
                }
            }

            // Combine and sort all tasks across jobs
            List<Task> allTasks = jobs.Values.SelectMany(job => job.Tasks).ToList();
            List<Task> sortedTasks = SortTasks(allTasks);

            // Print sorted tasks with JobID
            Console.WriteLine("Optimal Task Execution Order:");
            foreach (var task in sortedTasks)
            {
                Console.WriteLine($"JobID: {task.JobID}, TaskID: {task.TaskID}, Subdivision: {task.Subdivision}, Time: {task.Time}, Depends on: {string.Join(", ", task.DependsOn)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static List<Task> SortTasks(List<Task> allTasks)
    {
        // Sort by dependencies (tasks with no dependencies first) and then by time
        return allTasks
            .OrderBy(task => task.DependsOn.Count) // Tasks with fewer dependencies first
            .ThenBy(task => task.Time) // Shortest time among tasks with the same dependencies
            .ToList();
    }
}

public class Scheduler
{
    public static void ScheduleConcurrentTasks(List<Task>sortedTasks)
    {
        var taskQueue = new List<Task>();
    }
}

public class Task
{
    public int JobID { get; set; }
    public int TaskID { get; set; }
    public string Subdivision { get; set; }
    public int Time { get; set; }
    public List<string> DependsOn { get; private set; }

    public Task(int jobID, int taskID, string subdivision, int time)
    {
        JobID = jobID;
        TaskID = taskID;
        Subdivision = subdivision;
        Time = time;
        DependsOn = new List<string>();
    }

    public void AssignTaskDependencies(List<Task> allTasks)
    {
        // Gather subdivisions of tasks with IDs less than the current TaskID
        foreach (var task in allTasks)
        {
            if (task.TaskID < this.TaskID)
            {
                DependsOn.Add(task.Subdivision);
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Job.LoadJobsFromCSV();
    }
}
