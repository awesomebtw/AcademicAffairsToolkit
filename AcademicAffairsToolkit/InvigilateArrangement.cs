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

        public IntervalTree<DateTime, InvigilateRecordEntry> InvigilateRecords { get; private set; }

        public IntervalTree<DateTime, TROfficeRecordEntry> Constraints { get; private set; }

        public IEnumerable<TROfficeRecordEntry> TROfficeRecords { get; private set; }

        public int Iterations { get; private set; }

        public int PopulationCount { get; set; }

        // todo: result type
        public List<(TROfficeRecordEntry, InvigilateRecordEntry, int personCount)> Result { get; private set; }

        public GeneticAlgorithmScheduler(IEnumerable<InvigilateRecordEntry> invigilateRecords, IEnumerable<TROfficeRecordEntry> trOfficeRecords, IEnumerable<InvigilateConstraint> constraints, int iterations)
        {
            TROfficeRecords = trOfficeRecords;
            Iterations = iterations;

            foreach (var record in invigilateRecords)
            {
                InvigilateRecords.Add(record.StartTime, record.EndTime, record);
            }

            Constraints = new IntervalTree<DateTime, TROfficeRecordEntry>();
            foreach (var constraint in constraints)
            {
                Constraints.Add(constraint.From, constraint.To, constraint.TROffice);
            }
        }

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
        /// <returns>a tuple with office, invigilate task and people needed</returns>
        private IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> GeneratePermutation()
        {
            var invigilateList = InvigilateRecords.Select(p => (p.Value, GetInvigilatePersonCount(p.Value.ExamineeCount))).ToList();
            var trOfficeList = new List<TROfficeRecordEntry>(TROfficeRecords);
            var trOfficeRemaining = TROfficeRecords.Select(p => p.PeopleCount).ToList();

            Shuffle(ref invigilateList);

            Random random = new Random();

            // assign for each invigilate task
            for (int i = 0; i < invigilateList.Count; i++)
            {
                var (iRecord, peopleNeeded) = invigilateList[i];

                // ensure the selected office has enough people
                // by picking a random index and check if people is enough
                // to prevent endless loop when there's no office suitable,
                // a variable is added here to record the initial index
                int j = random.Next(trOfficeList.Count);
                int k = j;
                while (trOfficeRemaining[j] < peopleNeeded)
                {
                    j = (j + 1) % trOfficeList.Count;
                    if (j == k)
                        yield break; // todo: return with a fail flag
                }

                yield return Tuple.Create(trOfficeList[j], iRecord, peopleNeeded);

                // update remaining people list after assignment
                trOfficeRemaining[j] -= peopleNeeded;
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
        private IEnumerable<IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>>> GeneratePopulation(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GeneratePermutation();
            }
        }

        /// <summary>
        /// evaluate the chromosome's fitness according to constraints, etc
        /// </summary>
        /// <param name="chromosome">the chromosome to be evaluated</param>
        /// <returns>fitness of given chromosome</returns>
        private int GetFitness(IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> chromosome)
        {
            const int constarintViolationFactor = 150;
            const int arrangeFactor = 200;

            int fitness = 0;

            // see how many constraints are violated in population size times log(n)+m time
            fitness -= constarintViolationFactor * chromosome.Count(
                p => Constraints.Query(p.Item2.StartTime, p.Item2.EndTime).Contains(p.Item1));

            // check if people assigned is more than an office's people count
            fitness -= arrangeFactor * chromosome.GroupBy(p => p.Item1, p => p.Item3).Where(p => p.Key.PeopleCount < p.Sum()).Count();

            // todo: check if there's any time overlap

            // check if the arrangement is evenly spaced by evaluating the variance of time
            var group = chromosome.GroupBy(p => p.Item1, p => ((DateTimeOffset)p.Item2.StartTime).ToUnixTimeSeconds());
            foreach (var item in group)
            {
                var avg = item.Average();
                fitness += (int)(item.Sum(p => Math.Pow(p - avg, 2)) / item.Count());
            }

            return fitness;
        }

        private IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> ApplyCrossover(IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> left, IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> right)
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
            var population = GeneratePopulation(PopulationCount);
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
