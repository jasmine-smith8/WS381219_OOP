namespace WS381219_OOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Selection
    {
        private CalculateFitnessImplementation fitnessCalculator = new CalculateFitnessImplementation();
        public Job TournamentSelection(List<Job> population, int tournamentSize, Random random)
        {
            // Select a random subset of the population for tournament selection
            var tournament = new List<Job>();
            // Iterate through the tournament size
            for (int i = 0; i < tournamentSize; i++)
            {
                // Select a random job from the population
                int index = random.Next(population.Count);
                // Add the selected job to the tournament
                tournament.Add(population[index]);
            }
            // Calculate fitness for each job in the tournament
            tournament.ForEach(job => fitnessCalculator.CalculateFitness(job));
            // Select the job with the best fitness (minimum makespan)
            return tournament.MinBy(job => job.Fitness);
        }

        public Job GetBestJob(List<Job> population)
        {
            // Calculate fitness for each job in the population
            population.ForEach(job => fitnessCalculator.CalculateFitness(job));

            // Select and return the job with the best fitness (minimum makespan)
            return population.OrderBy(job => job.Fitness).FirstOrDefault();
        }
    }
}