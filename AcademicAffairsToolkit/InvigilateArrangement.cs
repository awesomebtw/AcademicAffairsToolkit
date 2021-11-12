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
        private static double CrossoverProbability = 0.75;

        private static double MutationProbobility = 0.1;

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
        /// generate a random permutation of arrangement
        /// </summary>
        /// <returns>a tuple with office, invigilate task and people needed</returns>
        private IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> GeneratePermutation()
        {
            var invigilateList = InvigilateRecords.Select(p => (record: p.Value, peopleNeeded: GetInvigilatePersonCount(p.Value.ExamineeCount))).ToList();
            var trOfficeList = new List<TROfficeRecordEntry>(TROfficeRecords);
            var trOfficeRemaining = TROfficeRecords.Select(p => p.PeopleCount).ToList();

            Shuffle(ref invigilateList);

            Random random = new Random();

            // assign for each invigilate task
            for (int i = 0; i < invigilateList.Count(); i++)
            {
                var (iRecord, peopleNeeded) = invigilateList[i];

                // ensure the selected office has enough people
                // by picking a random index and check if people is enough
                int j = random.Next(trOfficeList.Count);
                while (trOfficeRemaining[j] < peopleNeeded)
                    j = (j + 1) % trOfficeList.Count;

                yield return Tuple.Create(trOfficeList[j], iRecord, peopleNeeded);

                trOfficeRemaining[j] -= peopleNeeded; // subtract 
                if (trOfficeRemaining[j] == 0)
                {
                    trOfficeList.RemoveAt(j);
                    trOfficeRemaining.RemoveAt(j);
                }
            }
        }

        private IEnumerable<IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>>> GeneratePopulation(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GeneratePermutation();
            }
        }

        private int GetFitness(IEnumerable<Tuple<TROfficeRecordEntry, InvigilateRecordEntry, int>> chromosome)
        {
            // todo: fitness funtion evaluation
            return 0;
        }

        private void ApplyCrossover()
        {
            // todo: 
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
                var fitness = population.Select(p => GetFitness(p));
                if (random.NextDouble() < CrossoverProbability)
                    ApplyCrossover();
                if (random.NextDouble() < MutationProbobility)
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
