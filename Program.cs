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
        public static void Main(string[] args)
        {
            // Load jobs from CSV files
            var jobs = JobLoader.LoadJobsFromCSV();
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

            // Stop the stopwatch
            stopWatch.Stop();

            //Output the result
            Console.WriteLine($"Best job fitness: {bestJob.Fitness}");
            // Export the schedule to a CSV file
            var scheduler = new Scheduler();
            foreach (var task in bestJob.Tasks)
            {
                scheduler.ScheduleTask(task);
            }
            var schedule = new ScheduleExporter(scheduler);
            // Print the schedule
            schedule.PrintSchedule();
            // Ask user if they want to export the schedule
            Console.Write("Do you want to export the schedule to an XLSX file? (y/n): ");
            var exportChoice = Console.ReadLine();
            // If user chooses to export, proceed with export
            if (exportChoice?.ToLower() == "y")
            {
                // Allow user to name file
                Console.Write("Enter the name of the output file (without extension): ");
                var name = Console.ReadLine();
                schedule.ExportScheduleToXLSX($"{name}.xlsx");
            }
            else
            {
                Console.WriteLine("Schedule not exported.");
            }
            // Output the elapsed time
            Console.WriteLine($"Elapsed time: {stopWatch.ElapsedMilliseconds} ms ({stopWatch.Elapsed.TotalSeconds} seconds)");
        }
    }
}