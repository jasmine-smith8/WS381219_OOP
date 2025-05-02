namespace WS381219_OOP
{
    public class JobLoader
    {
        // Property to hold the selected file path
        public static string SelectedFile { get; private set; } = string.Empty;
        // Nested dictionary to hold job data so that each job can have multiple subdivisions and their respective processing times
        public static Dictionary<int, List<(int OperationID, string Subdivision, int ProcessingTime)>> LoadJobsFromCSV()
        {
            var jobs = new Dictionary<int, List<(int OperationID, string Subdivision, int ProcessingTime)>>();
            string[] files = Directory.GetFiles("jobs", "*.csv");

            Console.WriteLine("Select a job file:");

            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"{i + 1}: {files[i]}"); // Display files with an index starting from 1
            }

            while (true)
            {
                Console.Write("Enter the file number: ");
                if (int.TryParse(Console.ReadLine(), out int fileNumber) && fileNumber > 0 && fileNumber <= files.Length)
                {
                    string selectedFile = files[fileNumber - 1]; // Get the file based on the user's input
                    Console.WriteLine($"You selected: {selectedFile}");
                    SelectedFile = selectedFile; // Set the selected file for later use
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid file number.");
                }
            }

            if (!File.Exists(SelectedFile))
            {
                Console.WriteLine("File not found.");
                return jobs;
            }

            using (var reader = new StreamReader(SelectedFile))
            {
                string header = reader.ReadLine() ?? string.Empty; // Skip the header
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? string.Empty;
                    string[] cells = line.Split(',') ?? Array.Empty<string>();
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