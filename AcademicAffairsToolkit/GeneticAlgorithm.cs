using IntervalTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicAffairsToolkit
{
    class GeneticAlgorithm : IArrangementAlgorithm
    {
        private static readonly double crossoverProbability = 0.75;

        private static readonly double mutationProbability = 0.1;

        private readonly int resultSize = 10;

        private readonly TimeSpan longestExamTime = TimeSpan.Zero;

        private readonly InvigilateRecordEntry[] invigilateRecords;

        private readonly int[] peopleNeeded;

        private readonly IntervalTree<DateTime, TROfficeRecordEntry> constraints;

        private readonly TROfficeRecordEntry[] trOfficeRecords;

        private readonly int iterations;

        private readonly int populationSize;

        private readonly double invigilatePerCapita;

        private readonly CancellationToken cancellationToken;

        public event EventHandler<ArrangementStepForwardEventArgs> ArrangementStepForward;
        public event EventHandler<ArrangementTerminatedEventArgs> ArrangementTerminated;

        /// <summary>
        /// construct a new object for invigilate arrangement using genetic algorithm
        /// </summary>
        /// <param name="invigilateRecords">invigilate tasks to be assigned</param>
        /// <param name="trOfficeRecords">teaching and researching offices which will be </param>
        /// <param name="constraints">time constraints to be applied when evaluating arrangement</param>
        /// <param name="iterations">maximum iteration for arrangement</param>
        /// <param name="populationSize">population size for each generation</param>
        /// <param name="resultSize">how many solutions to retrieve</param>
        public GeneticAlgorithm(IEnumerable<InvigilateRecordEntry> invigilateRecords,
            IEnumerable<TROfficeRecordEntry> trOfficeRecords,
            IEnumerable<InvigilateConstraint> constraints,
            int iterations,
            int populationSize,
            int resultSize)
            : this(invigilateRecords,
                  trOfficeRecords,
                  constraints,
                  iterations,
                  populationSize,
                  resultSize,
                  CancellationToken.None)
        {
            
        }

        /// <summary>
        /// construct a new object for invigilate arrangement using genetic algorithm
        /// </summary>
        /// <param name="invigilateRecords">invigilate tasks to be assigned</param>
        /// <param name="trOfficeRecords">teaching and researching offices which will be </param>
        /// <param name="constraints">time constraints to be applied when evaluating arrangement</param>
        /// <param name="iterations">maximum iteration for arrangement</param>
        /// <param name="populationSize">population size for each generation</param>
        /// <param name="resultSize">how many solutions to retrieve</param>
        /// <param name="cancellationToken">cancellation token for asynchronous task</param>
        public GeneticAlgorithm(IEnumerable<InvigilateRecordEntry> invigilateRecords,
            IEnumerable<TROfficeRecordEntry> trOfficeRecords,
            IEnumerable<InvigilateConstraint> constraints,
            int iterations,
            int populationSize,
            int resultSize,
            CancellationToken cancellationToken)
        {
            if (populationSize <= 1)
                throw new ArgumentOutOfRangeException(nameof(populationSize), "population size must be greater than 1");

            this.trOfficeRecords = trOfficeRecords.ToArray();
            this.iterations = iterations;
            this.populationSize = populationSize;
            this.resultSize = resultSize;
            this.cancellationToken = cancellationToken;

            // sort invigilate records by time for convenience
            // no need to use SortedList
            this.invigilateRecords = invigilateRecords.OrderBy(p => p.StartTime).ThenBy(p => p.EndTime).ToArray();
            peopleNeeded = this.invigilateRecords.Select(p => GetInvigilatePersonCount(p.ExamineeCount)).ToArray();

            // some additional information used in the algorithm
            invigilatePerCapita = (double)peopleNeeded.Sum() / trOfficeRecords.Sum(p => p.PeopleCount);
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
            var trOfficeRemaining = trOfficeRecords.Select(p => p.PeopleCount).ToList();

            Random random = new Random();

            // assign for each invigilate task
            for (int i = 0; i < invigilateRecords.Length; i++)
            {
                // ensure the selected office has enough people
                // by picking a random index and check if people is enough
                // to prevent endless loop when there's no office suitable,
                // a variable is added here to record the initial index
                int j = random.Next(trOfficeRecords.Length);
                int k = j;
                while (trOfficeRemaining[j] < peopleNeeded[i])
                {
                    j = (j + 1) % trOfficeRecords.Length;
                    if (j == k)
                        throw new ApplicationException("available person is not sufficient for arrangement");
                }

                yield return trOfficeRecords[j];
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
            fitness -= constraintViolationFactor * invigilateRecords
                .Select((p, i) => constraints.Query(p.StartTime, p.EndTime).Count(p => p == chromosome[i]))
                .Sum();

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
            var invigilateCountDelta = trOfficeRecords.ToDictionary(p => p, p => p.PeopleCount * invigilatePerCapita);
            for (int i = 0; i < chromosome.Length; i++)
            {
                invigilateCountDelta[chromosome[i]] -= peopleNeeded[i];
            }
            fitness -= distributionFactor * (int)invigilateCountDelta.Values.Sum(p => Math.Floor(Math.Abs(p)));

            return fitness;
        }

        /// <summary>
        /// perform uniform crossover in place for the chromosome
        /// </summary>
        /// <param name="left">the first parent</param>
        /// <param name="right">the second parent</param>
        /// <exception cref="ArgumentException"></exception>
        private void ApplyCrossoverInplace(ref TROfficeRecordEntry[] left, ref TROfficeRecordEntry[] right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("two parent chromosomes must have the same length");

            Random random = new Random();
            for (int i = 0; i < left.Length; i++)
            {
                if (random.NextDouble() < 0.5)
                {
                    var temp = left[i];
                    left[i] = right[i];
                    right[i] = temp;
                }
            }
        }

        /// <summary>
        /// do mutation in place with inversion mutating
        /// </summary>
        /// <param name="chromosome">chromosome to be mutated</param>
        private void MutateInplace(ref TROfficeRecordEntry[] chromosome)
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
        }

        /// <summary>
        /// select a pair of gene using roulette-wheel algorithm for crossover and mutation
        /// </summary>
        /// <param name="fitness">array of evaluated fitness</param>
        /// <returns>a pair of integer representing selected indices of chromosome</returns>
        private (int, int) PerformSelection(in int[] fitness)
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
            int innerIterations = populationSize / 2; //< two offsprings will be generated for each iteration
            var population = GenerateInitialPopulation(populationSize).ToList();

            var fitnessDict = population.ToDictionary(p => p, p => GetFitness(p));
            var fitness = population.Select(p => fitnessDict[p]).ToArray();
            double fitnessAverage = fitness.Average();
            double fitnessAvgDev = fitness.Sum(p => Math.Abs(p - fitnessAverage)) / fitness.Length;

            double localCrossoverProbability = crossoverProbability;
            double localMutationProbabilty = mutationProbability;

            for (int i = 0; i < iterations; i++)
            {
                if (cancellationToken.IsCancellationRequested == true)
                    break;

                for (int j = 0; j < innerIterations; j++)
                {
                    (int index1, int index2) = PerformSelection(fitness);

                    var child1 = population[index1].ToArray();
                    var child2 = population[index2].ToArray();

                    if (random.NextDouble() < localCrossoverProbability)
                        ApplyCrossoverInplace(ref child1, ref child2);
                    if (random.NextDouble() < localMutationProbabilty)
                        MutateInplace(ref child1);
                    if (random.NextDouble() < localMutationProbabilty)
                        MutateInplace(ref child2);

                    int minFitness = fitness.Min();
                    int fitness1 = GetFitness(child1);
                    if (fitness1 > minFitness && fitnessDict.TryAdd(child1, fitness1))
                    {
                        population.Add(child1);
                    }

                    int fitness2 = GetFitness(child2);
                    if (fitness2 > minFitness && fitnessDict.TryAdd(child2, fitness2))
                    {
                        population.Add(child2);
                    }
                }

                // elinimate low-fitness population
                // use dictionary here to avoid repeated fitness evaluations
                var fitnessThreshold = fitnessDict.Values.OrderByDescending(p => p).Take(populationSize).Last();
                // Remove() will always return true here because elements are guarantee to be exist,
                // therefore we can dalete corresponding items from the dictionary when delete from population
                population.RemoveAll(p => fitnessDict[p] < fitnessThreshold && fitnessDict.Remove(p));
                // if two or more chromosomes has the same length, just arbitrarily choose chromosomes to remove
                for (int j = population.Count - 1; population.Count > populationSize && j >= 0; j--)
                {
                    if (fitnessDict[population[j]] == fitnessThreshold)
                        population.RemoveAt(j);
                }

                fitness = population.Select(p => fitnessDict[p]).ToArray();
                fitnessAverage = fitness.Average();
                fitnessAvgDev = fitness.Sum(p => Math.Abs(p - fitnessAverage)) / fitness.Length;

                // increase crossover and mutation probability to ensure diversity and optimal solution
                if (fitnessAvgDev < 0.01)
                {
                    localCrossoverProbability = Math.Min(localCrossoverProbability * 1.005, 0.9);
                    localMutationProbabilty = Math.Min(localMutationProbabilty * 1.005, 0.8);
                }

                Debug.WriteLine($"generation {i}, fitness max {fitness.Max()}, avg {fitnessAverage}, " +
                    $"avgdev {fitnessAvgDev}, crate {localCrossoverProbability}, mrate {localMutationProbabilty}");

                ArrangementStepForward?.Invoke(this, new ArrangementStepForwardEventArgs(i));
            }

            var maxFitness = fitness.Max();
            var result = population.Where((_, i) => fitness[i] == maxFitness).Distinct().Take(resultSize);

            ArrangementTerminated?.Invoke(this,
                new ArrangementTerminatedEventArgs(cancellationToken.IsCancellationRequested,
                    result, invigilateRecords, peopleNeeded));
        }

        public async Task StartArrangementAsync()
        {
            await Task.Run(StartArrangement, cancellationToken);
        }
    }
}
