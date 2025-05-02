using System.Collections.Generic;
using Xunit;
using WS381219_OOP;

namespace WS381219_OOP.Tests
{
    public class CrossoverImplementationTests
    {
        [Fact]
        public void Crossover_CombineWithNoDuplicates()
        {
            // Arrange
            var parent1 = new Job(1);
            parent1.AddTask(new Task(1, 1, "example", 10));
            parent1.AddTask(new Task(1, 2, "example", 15));

            var parent2 = new Job(1);
            parent2.AddTask(new Task(1, 2, "example", 15)); 
            parent2.AddTask(new Task(1, 3, "example", 20));

            var crossover = new CrossoverImplementation();

            // Act
            var child = crossover.Crossover(parent1, parent2);

            // Assert
            Assert.Equal(3, child.Tasks.Count);
            Assert.Contains(child.Tasks, t => t.OperationID == 1 && t.ProcessingTime == 10);
            Assert.Contains(child.Tasks, t => t.OperationID == 2 && t.ProcessingTime == 15);
            Assert.Contains(child.Tasks, t => t.OperationID == 3 && t.ProcessingTime == 20);
        }

        [Fact]
        public void Crossover_PreserveJobIDFromParents()
        {
            // Arrange
            var parent1 = new Job(2);
            parent1.AddTask(new Task(2, 1, "example", 10));

            var parent2 = new Job(2);
            parent2.AddTask(new Task(2, 2, "example", 20));

            var crossover = new CrossoverImplementation();

            // Act
            var child = crossover.Crossover(parent1, parent2);

            // Assert
            Assert.Equal(2, child.JobID); // Ensure JobID is preserved
        }

        [Fact]
        public void Crossover_SortTasksByOperationID()
        {
            // Arrange
            var parent1 = new Job(3);
            parent1.AddTask(new Task(3, 2, "example", 15));
            parent1.AddTask(new Task(3, 1, "example", 10));

            var parent2 = new Job(3);
            parent2.AddTask(new Task(3, 3, "example", 20));

            var crossover = new CrossoverImplementation();

            // Act
            var child = crossover.Crossover(parent1, parent2);

            // Assert
            Assert.Equal(3, child.Tasks.Count);
            Assert.Equal(1, child.Tasks[0].OperationID);
            Assert.Equal(2, child.Tasks[1].OperationID);
            Assert.Equal(3, child.Tasks[2].OperationID);
        }

        [Fact]
        public void Crossover_HandleEmptyParentJobs()
        {
            // Arrange
            var parent1 = new Job(4); // No tasks
            var parent2 = new Job(4); // No tasks

            var crossover = new CrossoverImplementation();

            // Act
            var child = crossover.Crossover(parent1, parent2);

            // Assert
            Assert.Empty(child.Tasks); // No tasks in the child
            Assert.Equal(4, child.JobID); // JobID should still be preserved
        }
    }    
}