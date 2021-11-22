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
        /// start the algorithm in asynchronized way
        /// </summary>
        /// <returns>task object representing the thread</returns>
        Task StartArrangementAsync();
    }

    class ArrangementStepForwardEventArgs : EventArgs
    {
        public int CurrentIteration { get; private set; }

        public ArrangementStepForwardEventArgs(int currentIteration)
        {
            CurrentIteration = currentIteration;
        }
    }

    class ArrangementTerminatedEventArgs : EventArgs
    {
        public bool Cancelled { get; private set; }

        public IEnumerable<TROfficeRecordEntry[]> Result { get; private set; }

        public InvigilateRecordEntry[] InvigilateRecords { get; private set; }

        public int[] PeopleNeeded { get; private set; }

        public ArrangementTerminatedEventArgs(bool cancelled, IEnumerable<TROfficeRecordEntry[]> result, InvigilateRecordEntry[] record, int[] peopleNeeded)
        {
            Cancelled = cancelled;
            Result = result;
            InvigilateRecords = record;
            PeopleNeeded = peopleNeeded;
        }
    }
}
