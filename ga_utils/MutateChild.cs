namespace WS381219_OOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MutateChild
    {
        private Random random = new Random();

        public void SetRandom(Random random)
        {
            this.random = random;
        }
        private CalculateFitnessImplementation fitnessCalculator = new CalculateFitnessImplementation();
        public void Mutate(Job child)
        {
            // Check if the job or its tasks are null or empty
            if (child == null || child.Tasks == null || child.Tasks.Count < 2)
            {
                Console.WriteLine("Mutation skipped: Job is empty or has less than two tasks.");
                return;
            }
            // Select two random indices for mutation
            int index1 = random.Next(child.Tasks.Count);
            int index2 = random.Next(child.Tasks.Count);

            // Ensure the tasks belong to the same job
            if (child.Tasks[index1].JobID == child.Tasks[index2].JobID)
            {
                // Check if the indices are different
                if (index1 == index2)
                {
                    // If they are the same, don't swap
                    Console.WriteLine("Mutation skipped: Selected tasks are the same.");
                    return;
                }
                if (index1 < 0 || index1 >= child.Tasks.Count || index2 < 0 || index2 >= child.Tasks.Count)
                {
                    Console.WriteLine("Invalid indices for mutation.");
                    return;
                }
                // Swap the tasks
                var temp = child.Tasks[index1];
                child.Tasks[index1] = child.Tasks[index2];
                child.Tasks[index2] = temp;

                // Recalculate fitness after mutation
                fitnessCalculator.CalculateFitness(child);
                Console.WriteLine("Mutation occurred.");
            }
        }
    }
}