using System;
using System.Collections.Generic;
using Xunit;
using WS381219_OOP;

namespace WS381219_OOP.Tests
{
    public class TournamentSelectionTests
    {
        [Fact]
        public void TournamentSelection_SelectsTaskWithBestFitness()
        {
            // Arrange
            var population = new List<Job>
            {
                new Job(1) { Fitness = 10 },
                new Job(2) { Fitness = 20 },
                new Job(3) { Fitness = 15 }
            };

            population[0].Tasks.Add(new Task(1,1,"Task A", 10));
            population[1].Tasks.Add(new Task(2,1,"Task B", 20));
            population[2].Tasks.Add(new Task(3,1,"Task C", 15));

            var random = new RandomStub(new List<int> { 0, 1, 2 });
            var selection = new Selection();

            // Act
            var selectedJob = selection.TournamentSelection(population, 3, random);

            // Assert
            Assert.Equal(10, selectedJob.Fitness); // Task with the best fitness (10)
        }

        [Fact]
        public void TournamentSelection_HandlesPopulationSmallerThanTournamentSize()
        {
            // Arrange
            var population = new List<Job>
            {
                new Job(1) { Fitness = 50 },
                new Job(2) { Fitness = 30 }
            };
            var random = new RandomStub(new List<int> { 0, 1, 0 }); 
            var selection = new Selection();

            // Act
            var selectedJob = selection.TournamentSelection(population, 3, random);

            // Assert
            Assert.NotNull(selectedJob);
            Assert.Contains(selectedJob, population);
        }

        [Fact]
        public void TournamentSelection_CalculatesFitnessForAllJobsInTournament()
        {
            // Arrange
            var population = new List<Job>
            {
                new Job(1),
                new Job(2),
                new Job(3)
            };
            var random = new RandomStub(new List<int> { 0, 1, 2 }); 
            var selection = new Selection();

            // Act
            selection.TournamentSelection(population, 3, random);

            // Assert
            foreach (var job in population)
            {
                Assert.True(job.Fitness >= 0, "Fitness should be a non-negative value.");
            }
        }

        [Fact]
        public void TournamentSelection_ThrowsExceptionForEmptyPopulation()
        {
            // Arrange
            var population = new List<Job>(); // Empty population
            var random = new Random();
            var selection = new Selection();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selection.TournamentSelection(population, 3, random));
        }

        // Helper class to mock Random
        private class RandomStub : Random
        {
            private readonly Queue<int> _indices;

            public RandomStub(IEnumerable<int> indices)
            {
                _indices = new Queue<int>(indices);
            }

            public override int Next(int maxValue)
            {
                return _indices.Dequeue();
            }
        }
    }
}