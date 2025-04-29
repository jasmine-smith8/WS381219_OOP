namespace WS381219_OOP
{
    public class Scheduler
    {
        // Schedule maps StartTime to Task
        // SortedDictionary is used to maintain the order of tasks based on their start time
        // Tuple<int, int> is used to represent the key (StartTime, OperationID)
        // Task is the value associated with the key
        public SortedDictionary<Tuple<int, int>, Task> Schedule { get; set; }
        // Dictionary to hold total makespan for overall schedule
        // Dictionary<int, int> is used to map machine ID to its makespan
        public Dictionary<int, int> Makespan { get; set; }

        public Scheduler()
        {
            Schedule = new SortedDictionary<Tuple<int, int>, Task>();
            Makespan = new Dictionary<int, int>();
        }

        // Method to add a task to the schedule
        public void ScheduleTask(Task task)
        {
            if (Schedule.Values.Any(existingTask => existingTask.StartTime == task.StartTime && existingTask.Subdivision == task.Subdivision))
            {
                Console.WriteLine($"Conflict detected: Task with StartTime {task.StartTime} already exists in subdivision {task.Subdivision}.");
            }
            else
            {
                Schedule[new Tuple<int, int>(task.StartTime, task.OperationID)] = task;
            }
        }
    }
}