
//WS381219-OOP
//Job Shop Scheduling Problem
using System;
using System.Collections.Generic;
using System.IO;
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
    public static void LoadJobsFromCSV()
    {
        var jobData = new Dictionary<int, Dictionary<int, string>>();
        string selectedJob;

        try
        {
            Console.WriteLine("Select the jobset from list: ");
            Console.WriteLine("1: example_job.csv");
            selectedJob = Console.ReadLine();

            string[] lines = File.ReadAllLines($"jobs/{selectedJob}");

            // Skip the header row by starting at index 1
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] cells = line.Split(',');

                if (cells.Length == 4)
                {
                    // Parse jobID and taskID as integers
                    int jobID = int.Parse(cells[0].Trim());
                    int taskID = int.Parse(cells[1].Trim()); // taskID should be in cells[1]
                    string subdivision = $" Subdivision: {cells[2].Trim()}";
                    string time = $" Time: {cells[3].Trim()}";
                    string details = subdivision + time;

                    // Add jobID to the dictionary if not already present
                    if (!jobData.ContainsKey(jobID))
                    {
                        jobData[jobID] = new Dictionary<int, string>();
                    }

                    // Add task details to the dictionary under the jobID
                    jobData[jobID][taskID] = details;
                }
            }

            foreach (var job in jobData)
            {
                Console.WriteLine($"JobID: {job.Key}");
                foreach (var task in job.Value)
                {
                    Console.WriteLine($"\tTaskID: {task.Key}, {task.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

}

class Program{
    static void Main(string[] args)
    {
        Job.LoadJobsFromCSV();
    }
}
