
using System;
using System.Collections.Generic;
using Xunit;
using WS381219_OOP;

namespace WS381219_OOP.Tests
{
    public class MutateChildTests
    {
        private Random _random = new Random();
        private void SetRandom(Random random)
        {
            _random = random;
        }

        [Fact]
        public void Mutate_SwapsTwoTasksWithinSameJob()
        {
            // Arrange
            var job = new Job(1);
            job.AddTask(new Task(1, 1, "Task1", 10));
            job.AddTask(new Task(1, 2, "Task2", 15));
            var mutateChild = new MutateChild();

            // Act
            mutateChild.Mutate(job);

            // Assert
            Assert.Equal(2, job.Tasks.Count);
            Assert.Contains(job.Tasks, t => t.OperationID == 1 && t.ProcessingTime == 10);
            Assert.Contains(job.Tasks, t => t.OperationID == 2 && t.ProcessingTime == 15);
            Assert.NotEqual(1, job.Tasks[0].OperationID); // Ensure tasks are swapped
        }

        [Fact] 

        public void Mutate_NoMutationForSingleTaskJob()
        {
            // Arrange
            var job = new Job(1);
            job.AddTask(new Task(1, 1, "Task1", 10));
            var mutateChild = new MutateChild();

            // Act
            mutateChild.Mutate(job);

            // Assert
            Assert.Single(job.Tasks); // No mutation should occur
            Assert.Equal(1, job.Tasks[0].OperationID);
        }

        [Fact]
        public void Mutate_NoMutationIfSameTaskSelected()
        {
            // Arrange
            var job = new Job(1);
            job.AddTask(new Task(1, 1, "Task1", 10));
            job.AddTask(new Task(1, 2, "Task2", 15));
            var mutateChild = new MutateChild();

            var random = new RandomStub(0, 0); //return the same index
            mutateChild.SetRandom(random);

            // Act
            mutateChild.Mutate(job);

            // Assert
            Assert.Equal(2, job.Tasks.Count);
            Assert.Equal(1, job.Tasks[0].OperationID);
            Assert.Equal(2, job.Tasks[1].OperationID);
        }

        [Fact]
        public void Mutate_HandlesEmptyJobGracefully()
        {
            // Arrange
            var job = new Job(1);
            var mutateChild = new MutateChild();

            // Act
            mutateChild.Mutate(job);

            // Assert
            Assert.Empty(job.Tasks); // No mutation should occur
        }

        [Fact]
        public void Mutate_HandlesMultipleJobsIndependently()
        {
            // Arrange
            var job1 = new Job(1);
            job1.AddTask(new Task(1, 1, "Task1", 10));
            job1.AddTask(new Task(1, 2, "Task2", 15));

            var job2 = new Job(2);
            job2.AddTask(new Task(2, 1, "Task3", 20));
            job2.AddTask(new Task(2, 2, "Task4", 25));

            var mutateChild = new MutateChild();

            // Act
            mutateChild.Mutate(job1);
            mutateChild.Mutate(job2);

            // Assert
            Assert.Equal(2, job1.Tasks.Count);
            Assert.Equal(2, job2.Tasks.Count);
            Assert.NotEqual(job1.Tasks[0].OperationID, job1.Tasks[1].OperationID);
            Assert.NotEqual(job2.Tasks[0].OperationID, job2.Tasks[1].OperationID);
        }

        [Fact]
        public void Mutate_EnsuresTaskOrderIsValidAfterMutation()
        {
            // Arrange
            var job = new Job(1);
            job.AddTask(new Task(1, 1, "Task1", 10));
            job.AddTask(new Task(1, 2, "Task2", 15));
            job.AddTask(new Task(1, 3, "Task3", 20));
            var mutateChild = new MutateChild();

            // Act
            mutateChild.Mutate(job);

            // Assert
            Assert.Equal(3, job.Tasks.Count);
            Assert.Contains(job.Tasks, t => t.OperationID == 1);
            Assert.Contains(job.Tasks, t => t.OperationID == 2);
            Assert.Contains(job.Tasks, t => t.OperationID == 3);
        }

        // Helper class to mock Random
        private class RandomStub : Random
        {
            private readonly int _index1;
            private readonly int _index2;
            private int _callCount = 0;

            public RandomStub(int index1, int index2)
            {
                _index1 = index1;
                _index2 = index2;
            }

            public override int Next(int maxValue)
            {
                _callCount++;
                return _callCount == 1 ? _index1 : _index2;
            }
        }
    }
}
