namespace WS381219_OOP
{
    public class JobLoader
    {
    // Nested dictionary to hold job data so that each job can have multiple subdivisions and their respective processing times
        public static Dictionary<int, List<(int OperationID, string Subdivision, int ProcessingTime)>> LoadJobsFromCSV()
        {
            var jobs = new Dictionary<int, List<(int OperationID, string Subdivision, int ProcessingTime)>>();
            string[] files = Directory.GetFiles("jobs", "*.csv");
            Console.WriteLine("Select a job file:");
            foreach (string file in files)
            {
                Console.WriteLine(file);
            }
            Console.Write("Enter the file name (without extension): ");

            string fileName = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine("Invalid file name.");
                return jobs;
            }
            
            string filePath = Path.Combine("jobs", fileName + ".csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return jobs;
            }

            using (var reader = new StreamReader(filePath))
            {
                string header = reader.ReadLine(); // Skip the header
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] cells = line.Split(',');

                    if (cells.Length != 4)
                    {
                        Console.WriteLine($"Skipping invalid line: {line}");
                        continue;
                    }
                    try
                    {
                        int jobID = int.Parse(cells[0].Trim());
                        int operationID = int.Parse(cells[1].Trim());
                        string subdivision = cells[2].Trim();
                        int processingTime = int.Parse(cells[3].Trim());

                        if (!jobs.ContainsKey(jobID))
                        {
                            jobs[jobID] = new List<(int OperationID, string Subdivision, int ProcessingTime)>();
                        }
                        jobs[jobID].Add((operationID, subdivision, processingTime));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing line: {line}. Exception: {ex.Message}");
                    }
                }
            }
        return jobs;
        }
    }
}