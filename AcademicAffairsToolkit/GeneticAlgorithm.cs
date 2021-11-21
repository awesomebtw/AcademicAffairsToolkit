using IntervalTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicAffairsToolkit
{
    class GeneticAlgorithm : IArrangementAlgorithm
    {
        private static readonly double crossoverProbability = 0.75;

        private static readonly double mutationProbability = 0.1;

        private static readonly int newOffspringForEachGeneration = 20;

        private readonly int resultSize = 10;

        private readonly TimeSpan longestExamTime = TimeSpan.Zero;

        private readonly InvigilateRecordEntry[] invigilateRecords;

        private readonly int[] peopleNeeded;

        private readonly IntervalTree<DateTime, TROfficeRecordEntry> constraints;

        private readonly TROfficeRecordEntry[] trOfficeRecords;

        private readonly int iterations;

        private readonly int populationSize;

        public event EventHandler<ArrangementStepForwardEventArgs> ArrangementStepForward;
        public event EventHandler<ArrangementTerminatedEventArgs> ArrangementTerminated;

        private IEnumerable<TROfficeRecordEntry[]> result;

        /// <summary>
        /// construct a new object for invigilate arrangement
        /// </summary>
        /// <param name="invigilateRecords">invigilate tasks to be assigned</param>
        /// <param name="trOfficeRecords">teaching and researching offices which will be </param>
        /// <param name="constraints">time constraints to be applied when evaluating arrangement</param>
        /// <param name="iterations">maximum iteration for arrangement</param>
        /// <param name="populationSize">population size for each generation</param>
        public GeneticAlgorithm(IEnumerable<InvigilateRecordEntry> invigilateRecords, IEnumerable<TROfficeRecordEntry> trOfficeRecords, IEnumerable<InvigilateConstraint> constraints, int iterations, int populationSize = 100, int resultSize = 10)
        {
            if (populationSize <= 1)
                throw new ArgumentOutOfRangeException(nameof(populationSize), "population size must be greater than 1");

            this.trOfficeRecords = trOfficeRecords.ToArray();
            this.iterations = iterations;
            this.populationSize = populationSize;
            this.resultSize = resultSize;

            // sort invigilate records by time for convenience
            // no need to use SortedList
            this.invigilateRecords = invigilateRecords.OrderBy(p => p.StartTime).ThenBy(p => p.EndTime).ToArray();
            peopleNeeded = this.invigilateRecords.Select(p => GetInvigilatePersonCount(p.ExamineeCount)).ToArray();

            longestExamTime = this.invigilateRecords.Max(p => p.EndTime - p.StartTime);

            // use interval tree for searching constraints to reduce time complexity
            this.constraints = new IntervalTree<DateTime, TROfficeRecordEntry>();
            foreach (var constraint in constraints)
            {
                this.constraints.Add(constraint.From, constraint.To, constraint.TROffice);
            }
        }

        /// <summary>
        /// get invigilate person needed for an exam according to requirement
        /// </summary>
        /// <param name="students">examinee count of an exam</param>
        /// <returns>count of person needed</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static int GetInvigilatePersonCount(int students)
        {
            if (students < 0)
                throw new ArgumentOutOfRangeException(nameof(students));
            else if (students < 70)
                return 2;
            else if (students < 100)
                return 3;
            else if (students < 150)
                return 4;
            else if (students < 180)
                return 5;
            else
                return 6;
        }

        /// <summary>
        /// generate a random permutation of arrangement regardless of user-defined constraints
        /// </summary>
        /// <returns>An array of tuple with office and people needed,
        /// whose length is <c>InvigilateRecords.Length</c> and
        /// invigilate task at <c>InvigilateRecords[i]</c> is assigned to <c>chromosome[i]</c>.</returns>
        private IEnumerable<TROfficeRecordEntry> GenerateChromosome()
        {
            var trOfficeList = new List<TROfficeRecordEntry>(trOfficeRecords);
            var trOfficeRemaining = trOfficeRecords.Select(p => p.PeopleCount).ToList();

            Random random = new Random();

            // assign for each invigilate task
            for (int i = 0; i < invigilateRecords.Length; i++)
            {
                // ensure the selected office has enough people
                // by picking a random index and check if people is enough
                // to prevent endless loop when there's no office suitable,
                // a variable is added here to record the initial index
                int j = random.Next(trOfficeList.Count);
                int k = j;
                while (trOfficeRemaining[j] < peopleNeeded[i])
                {
                    j = (j + 1) % trOfficeList.Count;
                    if (j == k)
                        throw new ApplicationException("available person is not sufficient for arrangement");
                }

                yield return trOfficeList[j];
            }
        }

        /// <summary>
        /// randomly generate a population (a group of arrangement) regardless of user-defined constraints
        /// </summary>
        /// <param name="count">size of the generated popullation</param>
        /// <returns>generated population</returns>
        private IEnumerable<TROfficeRecordEntry[]> GenerateInitialPopulation(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GenerateChromosome().ToArray();
            }
        }

        /// <summary>
        /// evaluate the chromosome's fitness according to constraints, etc
        /// </summary>
        /// <param name="chromosome">the chromosome to be evaluated</param>
        /// <returns>fitness of given chromosome</returns>
        private int GetFitness(TROfficeRecordEntry[] chromosome)
        {
            const int constraintViolationFactor = 200;
            const int arrangeOverlapFactor = 400;
            const int distributionFactor = 30;

            int fitness = 0;

            // see how many constraints are violated in chromosome.Length * log(n)+m time
            for (int i = 0; i < invigilateRecords.Length; i++)
            {
                var record = invigilateRecords[i];
                fitness -= constraintViolationFactor * constraints.Query(record.StartTime, record.EndTime).Count(p => p == chromosome[i]);
            }

            // check if there're too many invigilate tasks were assigned to an office at the same time
            // using sliding window algorithm
            var inWindow = new Dictionary<TROfficeRecordEntry, int>(chromosome.Length);
            int overlapCount = 0;
            TimeSpan currentRange = TimeSpan.Zero;
            TimeSpan longestRange = longestExamTime * 2;

            for (int left = 0, right = 0; right < chromosome.Length; )
            {
                if (!inWindow.TryAdd(chromosome[right], peopleNeeded[right]))
                    inWindow[chromosome[right]] += peopleNeeded[right];

                currentRange = invigilateRecords[right].EndTime - invigilateRecords[left].StartTime;
                right++;

                if (currentRange > longestRange)
                {
                    overlapCount += inWindow.Count(p => p.Key.PeopleCount < p.Value);
                }

                // there must be no overlaps when window range is twice as long as the longest exam time,
                // therefore, we can shrink the window.
                // todo: sometimes cause exception because key is not found
                while (currentRange > longestRange)
                {
                    inWindow[chromosome[left]] -= peopleNeeded[left];

                    left++;
                    currentRange = invigilateRecords[right].EndTime - invigilateRecords[left].StartTime;
                }
            }
            fitness -= arrangeOverlapFactor * overlapCount;

            // check if the arrangement is evenly assigned to every office by
            // evaluating difference between expected count considering invigilate per capita and actual count
            double invigilatePerCapita = (double)peopleNeeded.Sum() / trOfficeRecords.Sum(p => p.PeopleCount);
            var invigilateCountDelta = trOfficeRecords.ToDictionary(p => p, p => p.PeopleCount * invigilatePerCapita);
            for (int i = 0; i < chromosome.Length; i++)
            {
                invigilateCountDelta[chromosome[i]] -= peopleNeeded[i];
            }
            fitness -= distributionFactor * (int)invigilateCountDelta.Sum(p => Math.Floor(Math.Abs(p.Value)));

            return fitness;
        }

        /// <summary>
        /// perform uniform crossover for the chromosome
        /// </summary>
        /// <param name="left">the first parent</param>
        /// <param name="right">the second parent</param>
        /// <returns>two new chromosomes (offspring) generated by crossing over two parent chromosomes</returns>
        /// <exception cref="ArgumentException"></exception>
        private void ApplyCrossover(in TROfficeRecordEntry[] left, in TROfficeRecordEntry[] right, out TROfficeRecordEntry[] child1, out TROfficeRecordEntry[] child2)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("two parent chromosomes must have the same length");

            child1 = new TROfficeRecordEntry[left.Length];
            child2 = new TROfficeRecordEntry[right.Length];
            Random random = new Random();
            for (int i = 0; i < left.Length; i++)
            {
                if (random.NextDouble() < 0.5)
                {
                    child1[i] = left[i];
                    child2[i] = right[i];
                }
                else
                {
                    child1[i] = right[i];
                    child2[i] = left[i];
                }
            }
        }

        /// <summary>
        /// do mutation with inverse mutating
        /// </summary>
        /// <param name="chromosome">chromosome to be mutated</param>
        /// <returns>a new mutated chromosome</returns>
        private TROfficeRecordEntry[] Mutate(TROfficeRecordEntry[] chromosome)
        {
            Random random = new Random();

            int right = random.Next(chromosome.Length);
            int left = random.Next(right);
            while (right > left)
            {
                var temp = chromosome[left];
                chromosome[left] = chromosome[right];
                chromosome[right] = temp;
                left++;
                right--;
            }

            return chromosome;
        }

        /// <summary>
        /// select a pair of gene using roulette-wheel algorithm for crossover and mutation
        /// </summary>
        /// <param name="fitness">array of evaluated fitness</param>
        /// <returns>a pair of integer representing selected indices of chromosome</returns>
        private (int, int) PerformSelection(IEnumerable<int> fitness)
        {
            // normalize fitnesses
            int max = fitness.Max();
            int min = fitness.Min();
            double[] normalizedFitness = fitness.Select(p => (double)(p - min) / (max - min)).ToArray();
            double fitnessSum = normalizedFitness.Sum();

            int[] selectedIndices = new int[2];
            Random random = new Random();
            for (int i = 0; i < 2; i++)
            {
                double pos = random.NextDouble() * fitnessSum;
                double current = 0;

                for (int j = 0; j < normalizedFitness.Length; j++)
                {
                    current += normalizedFitness[j];
                    if (current > pos)
                    {
                        selectedIndices[i] = j;
                        break;
                    }
                }
            }

            return (selectedIndices[0], selectedIndices[1]);
        }

        public void StartArrangement()
        {
            Random random = new Random();
            var population = GenerateInitialPopulation(populationSize).ToList();
            var fitness = population.Select(p => GetFitness(p)).ToArray();

            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < newOffspringForEachGeneration; j++)
                {
                    (int index1, int index2) = PerformSelection(fitness);

                    if (random.NextDouble() < crossoverProbability)
                    {
                        ApplyCrossover(population[index1], population[index2], out var child1, out var child2);

                        population.Add(child1);
                        population.Add(child2);
                    }

                    if (random.NextDouble() < mutationProbability)
                    {
                        population.Add(Mutate(population[index1]));
                        population.Add(Mutate(population[index2]));
                    }
                }

                // elinimate low-fitness population
                // use dictionary here to avoid repeated fitness evaluations
                // if two or more chromosomes has the same length, just arbitrarily choose chromosomes to remove
                var fitnessDict = population.Distinct().ToDictionary(p => p, p => GetFitness(p));
                var fitnessThreshold = fitnessDict.Values.OrderByDescending(p => p).Take(populationSize).Last();
                population.RemoveAll(p => fitnessDict[p] < fitnessThreshold);
                for (int j = population.Count - 1; population.Count > populationSize && j >= 0; j--)
                {
                    if (fitnessDict[population[j]] == fitnessThreshold)
                        population.RemoveAt(j);
                }

                fitness = population.Select(p => fitnessDict[p]).ToArray();

                Debug.WriteLine($"generation {i}, max fitness {fitness.Max()}, avg fitness {fitness.Average()}");

                ArrangementStepForward?.Invoke(this, new ArrangementStepForwardEventArgs(i, iterations));
            }

            var maxFitness = fitness.Max();
            result = population.Where((p, i) => fitness[i] == maxFitness).Distinct().Take(resultSize);

            ArrangementTerminated?.Invoke(this, new ArrangementTerminatedEventArgs(false, result, invigilateRecords, peopleNeeded));
        }

        public async Task StartArrangementAsync()
        {
            await Task.Run(StartArrangement);
        }
    }
}
