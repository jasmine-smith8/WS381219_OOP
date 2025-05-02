namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        public void PrintSchedule(int columnWidth = 15, bool includeHeaders = true, Func<string, bool>? subdivisionFilter = null)
        {
            if (!_scheduler.Schedule.Any())
            {
                Console.WriteLine("The schedule is empty.");
                return;
            }
            
            // Adds unique subdivision headers to a list
            var subdivisions = GetSubdivisions();
            if (subdivisionFilter != null)
            {
                subdivisions = subdivisions.Where(subdivisionFilter).ToList();
            }

            // Include helper method from other file
            var timeRows = InitialiseTimeRows(subdivisions);
            PopulateTimeRows(timeRows, subdivisions);

            Console.WriteLine("Schedule:");
            Console.WriteLine(new string('-', columnWidth * (subdivisions.Count + 1)));

            if (includeHeaders)
            {
                Console.Write("Time".PadRight(columnWidth));
                foreach (var subdivision in subdivisions)
                {
                    // Abbreviate the subdivision name for display
                    var abbreviation = subdivision switch
                    {
                        "Surface Treatment" => "ST",
                        "Quality Control" => "QC",
                        _ => subdivision
                    };
                    Console.Write(abbreviation.PadRight(columnWidth));
                }
                Console.WriteLine();
                Console.WriteLine(new string('-', columnWidth * (subdivisions.Count + 1)));
            }
            // Print the schedule
            foreach (var time in timeRows.Keys.OrderBy(t => t))
            {
                Console.Write($"{time}".PadRight(columnWidth));
                foreach (var subdivision in subdivisions)
                {
                    var task = timeRows[time][subdivision];
                    if (task != null)
                    {
                        Console.Write($"Job {task.JobID} Op {task.OperationID}".PadRight(columnWidth));
                    }
                    else
                    {
                        Console.Write("".PadRight(columnWidth));
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine(new string('-', columnWidth * (subdivisions.Count + 1)));
        }
    }
}