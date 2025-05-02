using System.Collections.Generic;
using Xunit;

namespace WS381219_OOP.Tests
{
    public class JobTests
    {
        [Fact]
        public void Constructor_InitialisesJobCorrectly()
        {
            // Arrange
            int jobID = 1;

            // Act
            var job = new Job(jobID);

            // Assert
            Assert.Equal(jobID, job.JobID);
            Assert.Empty(job.Tasks);
        }

        [Fact]
        public void AddTask_AddsTaskToJob()
        {
            // Arrange
            var job = new Job(1);
            var task = new Task(1, 1, "Task A", 10);

            // Act
            job.AddTask(task);

            // Assert
            Assert.Single(job.Tasks);
            Assert.Contains(task, job.Tasks); 
        }

        [Fact]
        public void Constructor_InitialisesJobStackCorrectly()
        {
            // Arrange
            var job = new Job(1);
            var task1 = new Task(1, 1, "Task A", 10);
            var task2 = new Task(1, 2, "Task B", 15);
            var unrelatedTask = new Task(2, 1, "Task C", 20);

            job.AddTask(task1);
            job.AddTask(task2);
            job.AddTask(unrelatedTask);

            // Act
            var jobStack = new Stack<Task>(job.Tasks.Where(task => task.JobID == job.JobID));

            // Assert
            Assert.Equal(2, jobStack.Count); // Only tasks with matching JobID should be in the stack
            Assert.Contains(task1, jobStack);
            Assert.Contains(task2, jobStack);
            Assert.DoesNotContain(unrelatedTask, jobStack);
        }
    }
}