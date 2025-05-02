namespace WS381219_OOP
{
    public class Task
    {
        // Properties of the Task class
        public int JobID { get; set; }           // ID of the job
        public int OperationID { get; set; }        // ID of the machine
        public string Subdivision { get; set; }     // Machine name 
        public int ProcessingTime { get; set; }     // Time the task takes
        public int StartTime { get; set; }          // When the task starts
        public int EndTime => StartTime + ProcessingTime; // Calculated automatically

        public Task(int jobID, int operationID, string subdivision, int processingTime)
        {
            JobID = jobID;
            OperationID = operationID;
            Subdivision = subdivision;
            ProcessingTime = processingTime;
        }
    }
}