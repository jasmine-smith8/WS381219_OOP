namespace WS381219_OOP
{
    public class Job
    {
        // jobID represents the unique identifier for each job
        public int JobID { get; set; }
        // Tasks is a list of tasks associated with the job
        public List<Task> Tasks { get; set; }
        // Fitness is used to evaluate the quality of the job schedule
        public double Fitness { get; set; }
        // Stack to store jobs in correct task order
        private Stack<Task> jobStack;

        // Constructor to initialise a job with a unique ID
        public Job(int jobID)
        {
            JobID = jobID;
            Tasks = new List<Task>();
            // Initialise the jobStack here
            jobStack = new Stack<Task>(Tasks.Where(task => task.JobID == JobID));
        }
        // Method to add a task to the job
        public void AddTask(Task task)
        {
            // Add the task to the list of tasks
            Tasks.Add(task);
        }
    }
}