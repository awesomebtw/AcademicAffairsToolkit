using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcademicAffairsToolkit
{
    interface IArrangementAlgorithm
    {
        /// <summary>
        /// will be raised when an iteration of the algorithm is finished
        /// </summary>
        public event EventHandler<ArrangementStepForwardEventArgs> ArrangementStepForward;

        /// <summary>
        /// will be raised when the algorithm is terminated
        /// </summary>
        public event EventHandler<ArrangementTerminatedEventArgs> ArrangementTerminated;

        /// <summary>
        /// start the algorithm in synchronized way
        /// </summary>
        void StartArrangement();

        /// <summary>
        /// start the algorithm in asynchronized, maybe multi-threaded way
        /// </summary>
        /// <returns>task object representing the thread</returns>
        Task StartArrangementAsync();
    }

    class ArrangementStepForwardEventArgs : EventArgs
    {
        public int CurrentIteration { get; private set; }

        public int TotalIterations { get; private set; }

        public ArrangementStepForwardEventArgs(int currentIteration, int totalIterations)
        {
            CurrentIteration = currentIteration;
            TotalIterations = totalIterations;
        }
    }

    class ArrangementTerminatedEventArgs : EventArgs
    {
        public bool HasFault { get; private set; }

        public IEnumerable<TROfficeRecordEntry[]> Result { get; private set; }

        public InvigilateRecordEntry[] InvigilateRecords { get; private set; }

        public int[] PeopleNeeded { get; private set; }

        public ArrangementTerminatedEventArgs(bool hasFault, IEnumerable<TROfficeRecordEntry[]> result, InvigilateRecordEntry[] record, int[] peopleNeeded)
        {
            HasFault = hasFault;
            Result = result;
            InvigilateRecords = record;
            PeopleNeeded = peopleNeeded;
        }
    }
}
