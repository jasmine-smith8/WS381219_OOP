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
}