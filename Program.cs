//WS381219-OOP
//Job Shop Scheduling Problem

using System;

public class Job
{
    public string JobID { get; set; }
    public DateTime DueDate { get; set; }
    public List<Task> Tasks { get; private set; }

    public Job(string jobID, DateTime dueDate)
    {
        JobID = jobID;
        DueDate = dueDate;
        Tasks = new List<Task>();
    }

    public void AddTask(Task task)
    {
        Tasks.Add(task);
    }

    public int GetTotalDuration()
    {
        int totalDuration = 0;

        foreach (var task in Tasks)
        {
            totalDuration += task.Duration;
        }

        return totalDuration;
    }

    public static List<Job> LoadJobsFromCSV(string filePath, Dictionary<string, Task> taskLookup)
    {
        List<Job> jobs = new List<Job>();

        return jobs;
    }

}

public class Task
{
    public string TaskID { get; set; }
    public int Duration { get; set; }
    public List<Task> Dependencies { get; private set; }
    public Subdivision AssignedSubdivision { get; set; }

    public Task(string taskID, int duration)
    {
        TaskID = taskID;
        Duration = duration;
        Dependencies = new List<Task>();
        AssignedSubdivision = null;
    }

    public void AddDependency(Task task)
    {
        Dependencies.Add(task);
    }

    public static Dictionary<string, Task> LoadTasksFromCSV(string filePath)
    {

    }
}

public class Subdivision
{
    public string Name { get; set; }
    public int ResourceCapacity { get; set; }

    public Subdivision(string name, int resourceCapacity)
    {
        Name = name;
        ResourceCapacity = resourceCapacity;
        AssignedTasks = new List<Task>();
    }

    public bool AssignTask(Task task)
    {
        AssignedTasks.Add(task);
    }
    
    public bool RemoveTask(Task task)
    {
        AssignedTasks.Remove(task);
    }

    public bool IsAtCapacity()
    {
        return AssignedTasks.Count >= ResourceCapacity;
    }

    public string GetAssignedTasks()
    {
        if (AssignedTasks.Count == 0)
        {
            return "No tasks currently assigned.";
        }

        return string.Join(", ", AssignedTasks.ConvertAll(task => task.TaskID));
    }
}

public class Scheduler
{
    public List<Task> Tasks { get; private set; }
    public List<Job> Jobs { get; private set; }
    public List<Subdivision> Subdivisions { get; private set; }

    public Scheduler()
    {
        Tasks = new List<Task>();
        Jobs = new List<Job>();
        Subdivisions = new List<Subdivision>();
    }

    public int OptimizeMakespan()
    {

    }

    public bool ValidateDependencies()
    {

    }

    public bool AssignTaskToSubdivision(Task task, Subdivision subdivision)
    {

    }

}