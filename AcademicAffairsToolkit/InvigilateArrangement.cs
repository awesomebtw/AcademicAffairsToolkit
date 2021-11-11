using IntervalTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IEnumerable<TROfficeRecordEntry> TROfficeRecords { get; private set; }

        public int Iterations { get; private set; }

        public int Population { get; set; }

        // todo: result type
        public List<(TROfficeRecordEntry, InvigilateRecordEntry, int personCount)> Result { get; private set; }

        public GeneticAlgorithmScheduler(IEnumerable<InvigilateRecordEntry> invigilateRecords, IEnumerable<TROfficeRecordEntry> trOfficeRecords, int iterations)
        {
            TROfficeRecords = trOfficeRecords;
            Iterations = iterations;

            foreach (var record in invigilateRecords)
            {
                InvigilateRecords.Add(record.StartTime, record.EndTime, record);
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
            var trOfficeList = TROfficeRecords.Select(p => (record: p, peopleRemaining: p.PeopleCount)).ToList();

            Shuffle(ref invigilateList);

            Random random = new Random();

            for (int i = 0; i < invigilateList.Count(); i++)
            {
                var (record, peopleNeeded) = invigilateList[i];

                // ensure the selected office has enough people
                // by picking a random index and check if people is enough
                int j = random.Next(trOfficeList.Count);
                int increment = random.Next(trOfficeList.Count);
                while (trOfficeList[j].peopleRemaining < peopleNeeded)
                    j = (j + increment) % trOfficeList.Count;

                var t = trOfficeList[j];

                yield return Tuple.Create(t.record, record, peopleNeeded);

                t.peopleRemaining -= peopleNeeded; // subtract 
                if (t.peopleRemaining == 0)
                    trOfficeList.Remove(t);
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
            var population = GeneratePopulation(Population);
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
