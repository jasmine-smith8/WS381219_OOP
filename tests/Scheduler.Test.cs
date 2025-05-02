using System;
using System.Collections.Generic;
using Xunit;

namespace WS381219_OOP.Tests
{
    public class SchedulerTests
    {
        [Fact]
        public void Constructor_InitialisesSchedulerCorrectly()
        {
            // Arrange & Act
            var scheduler = new Scheduler();

            // Assert
            Assert.Empty(scheduler.Schedule); // Schedule should be empty initially
            Assert.Empty(scheduler.Makespan); // Makespan should be empty initially
        }

        [Fact]
        public void ScheduleTask_AddsTaskToSchedule()
        {
            // Arrange
            var scheduler = new Scheduler();
            var task = new Task(1, 1, "Task A", 10)
            {
                StartTime = 5,
                Subdivision = "Subdivision1"
            };

            // Act
            scheduler.ScheduleTask(task);

            // Assert
            Assert.Single(scheduler.Schedule); // Schedule should contain one task
            Assert.Contains(new Tuple<int, int>(task.StartTime, task.OperationID), scheduler.Schedule.Keys);
            Assert.Equal(task, scheduler.Schedule[new Tuple<int, int>(task.StartTime, task.OperationID)]);
        }

        [Fact]
        public void ScheduleTask_DetectsConflictAndDoesNotAddTask()
        {
            // Arrange
            var scheduler = new Scheduler();
            var task1 = new Task(1, 1, "Task A", 10)
            {
                StartTime = 5,
                Subdivision = "Subdivision1"
            };
            var task2 = new Task(2, 2, "Task B", 15)
            {
                StartTime = 5,
                Subdivision = "Subdivision1" // Same StartTime and Subdivision as task1
            };

            scheduler.ScheduleTask(task1);

            // Act
            scheduler.ScheduleTask(task2);

            // Assert
            Assert.Single(scheduler.Schedule); // Task2 should not be added
            Assert.Contains(new Tuple<int, int>(task1.StartTime, task1.OperationID), scheduler.Schedule.Keys);
            Assert.DoesNotContain(new Tuple<int, int>(task2.StartTime, task2.OperationID), scheduler.Schedule.Keys);
        }
    }
}