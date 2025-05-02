namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        /// This list contains the unique subdivisions from the schedule.
        private List<string> GetSubdivisions()
        {
            return _scheduler.Schedule.Values
                .Select(task => task.Subdivision)
                .Distinct()
                .ToList();
        }
        private Dictionary<int, Dictionary<string, Task>> InitialiseTimeRows(List<string> subdivisions)
        {
            // Initialise the time rows dictionary with the start and end times of the tasks
            int minStartTime = _scheduler.Schedule.Keys.Min(key => key.Item1);
            int maxStartTime = _scheduler.Schedule.Keys.Max(key => key.Item1);

            // Create a dictionary to hold the time rows
            var timeRows = new Dictionary<int, Dictionary<string, Task>>();
            for (int time = minStartTime; time <= maxStartTime; time++)
            {
                // For each time, create a dictionary for each subdivision
                timeRows[time] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task?)null);
            }

            return timeRows;
        }

        private void PopulateTimeRows(Dictionary<int, Dictionary<string, Task>> timeRows, List<string> subdivisions)
        {
            // Boundary check for the time rows
            int maxStartTime = timeRows.Keys.Max();

            foreach (var entry in _scheduler.Schedule)
            {
                // Get the start time and task from the entry
                int startTime = entry.Key.Item1;
                var task = entry.Value;

                // Check if the task is in the current subdivision
                var previousTask = _scheduler.Schedule.Values
                    .Where(t => t.JobID == task.JobID && t.OperationID < task.OperationID)
                    .OrderByDescending(t => t.OperationID)
                    .FirstOrDefault();

                // If a previous task exists, adjust the start time to avoid conflicts
                if (previousTask != null && previousTask.EndTime > startTime)
                {
                    startTime = previousTask.EndTime;
                }
                // Check for conflicts with task start times in the same subdivision
                while (startTime > maxStartTime)
                {
                    maxStartTime++;
                    timeRows[maxStartTime] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task?)null);
                }
                // Add the task to the time rows dictionary
                timeRows[startTime][task.Subdivision] = task;
            }
            // Check for any tasks that may have been scheduled after the max start time
            int maxEncounteredStartTime = _scheduler.Schedule.Values.Max(t => t.EndTime);
            while (maxEncounteredStartTime > maxStartTime)
            {
                // If the max encountered start time exceeds the current max start time, add a new entry
                maxStartTime++;
                timeRows[maxStartTime] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task?)null);
            }
        }
    }
}