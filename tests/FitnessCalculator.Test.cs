using System.Collections.Generic;
using Xunit;
using WS381219_OOP;

namespace WS381219_OOP.Tests
{
    public class FitnessCalculatorTests
    {
        [Fact]
        public void CalculateFitness_ValidJob_ReturnsCorrectMakespan()
        {
            // Arrange
            var job = new Job(1);
            job.AddTask(new Task(1, 1, "example", 10));
            job.AddTask(new Task(1, 2, "example", 15));
            job.AddTask(new Task(1, 3, "example", 20));

            var fitnessCalculator = new CalculateFitnessImplementation();

            // Act
            fitnessCalculator.CalculateFitness(job);

            // Assert
            Assert.Equal(45, job.Fitness);
        }

        [Fact]
        public void CalculateFitness_EmptyJob_ReturnsZero()
        {
            // Arrange
            var job = new Job(2);
            var fitnessCalculator = new CalculateFitnessImplementation();

            // Act
            fitnessCalculator.CalculateFitness(job);

            // Assert
            Assert.Equal(0, job.Fitness); // Empty job should have zero makespan
        }
    }
}