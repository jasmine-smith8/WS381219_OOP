namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        private List<string> GetSubdivisions()
        {
            return _scheduler.Schedule.Values
                .Select(task => task.Subdivision)
                .Distinct()
                .ToList();
        }

        private Dictionary<int, Dictionary<string, Task>> InitializeTimeRows(List<string> subdivisions)
        {
            int minStartTime = _scheduler.Schedule.Keys.Min(key => key.Item1);
            int maxStartTime = _scheduler.Schedule.Keys.Max(key => key.Item1);

            var timeRows = new Dictionary<int, Dictionary<string, Task>>();
            for (int time = minStartTime; time <= maxStartTime; time++)
            {
                timeRows[time] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task)null);
            }

            return timeRows;
        }

        private void PopulateTimeRows(Dictionary<int, Dictionary<string, Task>> timeRows, List<string> subdivisions)
        {
            int maxStartTime = timeRows.Keys.Max();

            foreach (var entry in _scheduler.Schedule)
            {
                int startTime = entry.Key.Item1;
                var task = entry.Value;

                var previousTask = _scheduler.Schedule.Values
                    .Where(t => t.JobID == task.JobID && t.OperationID < task.OperationID)
                    .OrderByDescending(t => t.OperationID)
                    .FirstOrDefault();

                if (previousTask != null && previousTask.EndTime > startTime)
                {
                    startTime = previousTask.EndTime;
                }

                while (startTime > maxStartTime)
                {
                    maxStartTime++;
                    timeRows[maxStartTime] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task)null);
                }

                timeRows[startTime][task.Subdivision] = task;
            }

            int maxEncounteredStartTime = _scheduler.Schedule.Values.Max(t => t.EndTime);
            while (maxEncounteredStartTime > maxStartTime)
            {
                maxStartTime++;
                timeRows[maxStartTime] = subdivisions.ToDictionary(subdivision => subdivision, _ => (Task)null);
            }
        }
    }
}