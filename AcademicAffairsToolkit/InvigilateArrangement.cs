using IntervalTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicAffairsToolkit
{
    abstract class InvigilateArrangement
    {
    }

    class GeneticAlgorithmScheduler : InvigilateArrangement
    {
        private static readonly double crossoverProbability = 0.75;

        private static readonly double mutationProbobility = 0.1;

        public InvigilateRecordEntry[] InvigilateRecords { get; private set; }

        public int[] PeopleNeeded { get; set; }

        public IntervalTree<DateTime, TROfficeRecordEntry> Constraints { get; private set; }

        public IEnumerable<TROfficeRecordEntry> TROfficeRecords { get; private set; }

        public int Iterations { get; private set; }

        public int PopulationCount { get; set; }

        public IEnumerable<(TROfficeRecordEntry, InvigilateRecordEntry, int personCount)> Result { get; private set; }

        /// <summary>
        /// construct a new object for invigilate arrangement
        /// </summary>
        /// <param name="invigilateRecords"></param>
        /// <param name="trOfficeRecords">teaching and researching offices to be assigned</param>
        /// <param name="constraints">time constraints to be applied when evaluating arrangement</param>
        /// <param name="iterations">maximum iteration for arrangement</param>
        public GeneticAlgorithmScheduler(IEnumerable<InvigilateRecordEntry> invigilateRecords, IEnumerable<TROfficeRecordEntry> trOfficeRecords, IEnumerable<InvigilateConstraint> constraints, int iterations)
        {
            TROfficeRecords = trOfficeRecords;
            Iterations = iterations;

            // sort invigilate records by time for convenience
            // no need to use SortedList
            InvigilateRecords = invigilateRecords.OrderBy(p => p.StartTime).ThenBy(p => p.EndTime).ToArray();
            PeopleNeeded = invigilateRecords.Select(p => GetInvigilatePersonCount(p.ExamineeCount)).ToArray();

            // use interval tree for searching constraints to reduce time complexity
            Constraints = new IntervalTree<DateTime, TROfficeRecordEntry>();
            foreach (var constraint in constraints)
            {
                Constraints.Add(constraint.From, constraint.To, constraint.TROffice);
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

        private static void Shuffle<T>(ref List<T> list)
        {
            Random random = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                int selected = random.Next(i, list.Count);
                T temp = list[selected];
                list[selected] = list[i];
                list[i] = temp;
            }
        }

        /// <summary>
        /// generate a random permutation of arrangement regardless of user-defined constraints
        /// </summary>
        /// <returns>An array of tuple with office and people needed,
        /// whose length is InvigilateRecords.Length and
        /// invigilate task at InvigilateRecords[i] is assigned to chromosome[i].</returns>
        private IEnumerable<Tuple<TROfficeRecordEntry, int>> GenerateChromosome()
        {
            var trOfficeList = new List<TROfficeRecordEntry>(TROfficeRecords);
            var trOfficeRemaining = TROfficeRecords.Select(p => p.PeopleCount).ToList();

            Random random = new Random();

            // assign for each invigilate task
            for (int i = 0; i < InvigilateRecords.Length; i++)
            {
                // ensure the selected office has enough people
                // by picking a random index and check if people is enough
                // to prevent endless loop when there's no office suitable,
                // a variable is added here to record the initial index
                int j = random.Next(trOfficeList.Count);
                int k = j;
                while (trOfficeRemaining[j] < PeopleNeeded[i])
                {
                    j = (j + 1) % trOfficeList.Count;
                    if (j == k)
                        throw new ApplicationException("available person is not sufficient for arrangement");
                }

                yield return Tuple.Create(trOfficeList[j], PeopleNeeded[i]);

                // update remaining people list after assignment
                trOfficeRemaining[j] -= PeopleNeeded[i];
                if (trOfficeRemaining[j] < 2)
                {
                    trOfficeList.RemoveAt(j);
                    trOfficeRemaining.RemoveAt(j);
                }
            }
        }

        /// <summary>
        /// randomly generate a population (a group of arrangement) regardless of user-defined constraints
        /// </summary>
        /// <param name="count">size of the generated popullation</param>
        /// <returns>generated population</returns>
        private IEnumerable<Tuple<TROfficeRecordEntry, int>[]> GenerateInitialPopulation(int count)
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
        private int GetFitness(in Tuple<TROfficeRecordEntry, int>[] chromosome)
        {
            const int constarintViolationFactor = 150;
            const int arrangeFactor = 200;
            const int distributionFactor = 100;

            int fitness = 0;

            // see how many constraints are violated in population size times log(n)+m time
            for (int i = 0; i < InvigilateRecords.Length; i++)
            {
                var record = InvigilateRecords[i];
                if (Constraints.Query(record.StartTime, record.EndTime).Contains(chromosome[i].Item1))
                    fitness -= constarintViolationFactor;
            }

            // check if people assigned is more than an office's people count
            fitness -= arrangeFactor * chromosome.GroupBy(p => p.Item1, p => p.Item2)
                .Count(p => p.Key.PeopleCount < p.Sum());

            // since the invigilate record is sorted by start and end time,
            // we can check if the arrangement is evenly spaced by evaluating how many repetitions in the chromosome
            List<int> repetitionItems = new List<int>(chromosome.Length);
            for (int i = 1; i < chromosome.Length; i++)
            {
                if (chromosome[i - 1].Item1 == chromosome[i].Item1)
                    repetitionItems[^1]++;
                else
                    repetitionItems.Add(0);
            }

            fitness -= distributionFactor * repetitionItems.Sum();

            return fitness;
        }

        private IEnumerable<Tuple<TROfficeRecordEntry, int>> ApplyCrossover(IEnumerable<Tuple<TROfficeRecordEntry, int>> left, IEnumerable<Tuple<TROfficeRecordEntry, int>> right)
        {
            // todo: do crossover
            yield break;
        }

        private void Mutate()
        {
            // todo: mutation
        }

        public void StartArrangement()
        {
            // todo: algorithm
            Random random = new Random();
            var population = GenerateInitialPopulation(PopulationCount);
            for (int i = 0; i < Iterations; i++)
            {
                var fitness = population.OrderByDescending(p => GetFitness(p));
                if (random.NextDouble() < crossoverProbability)
                    ApplyCrossover(population.ElementAt(0), population.ElementAt(1));
                if (random.NextDouble() < mutationProbobility)
                    Mutate();
                // replace the old population with new one
            }
        }

        public async Task StartArrangementAsync()
        {
            // todo: concurrent version of the algorithm
            await Task.Run(StartArrangement);
        }
    }
}
