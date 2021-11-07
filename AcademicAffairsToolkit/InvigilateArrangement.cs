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
        public IEnumerable<InvigilateRecordEntry> InvigilateRecords { get; private set; }

        public IEnumerable<TROfficeRecordEntry> TROfficeRecords { get; private set; }

        public int Iterations { get; private set; }

        // todo: result type
        public List<(int trOfficeId, int invigilateId, int personCount)> Result { get; private set; }

        public GeneticAlgorithmScheduler(IEnumerable<InvigilateRecordEntry> invigilateRecords, IEnumerable<TROfficeRecordEntry> trOfficeRecords, int iterations)
        {
            InvigilateRecords = invigilateRecords;
            TROfficeRecords = trOfficeRecords;
            Iterations = iterations;
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

        private IEnumerable<List<(int, int, int)>> GenerateInitialPopulation(int count)
        {
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                List<(int, int, int)> initialChromosome = new List<(int, int, int)>();
                var invigilateRemaining = new Dictionary<int, int>(InvigilateRecords.Select((p, i) => new KeyValuePair<int, int>(GetInvigilatePersonCount(p.ExamineeCount), i)));
                var trOfficeRemaining = TROfficeRecords.Select(p => p.PeopleCount).ToArray();

                for (int j = 0; j < InvigilateRecords.Count(); j++)
                {
                    int selected = random.Next(0, trOfficeRemaining.Count() - 1);
                    initialChromosome.Add((selected, 0, 0));
                    trOfficeRemaining[selected] -= invigilateRemaining[selected];
                    invigilateRemaining.Remove(selected);
                }

                yield return initialChromosome;
            }
        }

        private int GetFitness()
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
            GenerateInitialPopulation(1);
            for (int i = 0; i < Iterations; i++)
            {
                GetFitness();
                ApplyCrossover();
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
