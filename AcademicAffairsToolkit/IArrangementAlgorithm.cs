using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicAffairsToolkit
{
    interface IArrangementAlgorithm
    {
        /// <summary>
        /// report progress of the algorithm
        /// </summary>
        public IProgress<int> ProgressReporter { get; set; }

        /// <summary>
        /// will be raised when the algorithm is terminated
        /// </summary>
        public event EventHandler<ArrangementTerminatedEventArgs> ArrangementTerminated;

        /// <summary>
        /// start the algorithm in synchronized way
        /// </summary>
        void StartArrangement();

        /// <summary>
        /// start the algorithm in asynchronized way
        /// </summary>
        /// <returns>task object representing the thread</returns>
        Task StartArrangementAsync();
    }

    class ArrangementTerminatedEventArgs : EventArgs
    {
        public bool Cancelled { get; private set; }

        public bool PotentialUnableToArrange { get; private set; }

        public IEnumerable<TROfficeRecordEntry[]> Result { get; private set; }

        public InvigilateRecordEntry[] InvigilateRecords { get; private set; }

        public int[] PeopleNeeded { get; private set; }

        public ArrangementTerminatedEventArgs(bool cancelled, bool potentialUnableToArrange, IEnumerable<TROfficeRecordEntry[]> result, InvigilateRecordEntry[] record, int[] peopleNeeded)
        {
            Cancelled = cancelled;
            PotentialUnableToArrange = potentialUnableToArrange;
            Result = result;
            InvigilateRecords = record;
            PeopleNeeded = peopleNeeded;
        }
    }
}
