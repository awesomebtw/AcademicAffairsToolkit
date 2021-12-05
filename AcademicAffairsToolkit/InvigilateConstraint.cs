using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AcademicAffairsToolkit
{
    class InvigilateConstraint : IEquatable<InvigilateConstraint>, INotifyPropertyChanged, IEditableObject
    {
        private DateTime from;
        private DateTime to;
        private TROfficeRecordEntry trOffice;

        public DateTime From
        {
            get { return from; }
            set
            {
                from = value;
                OnPropertyChanged();
            }
        }

        public DateTime To
        {
            get { return to; }
            set
            {
                to = value;
                OnPropertyChanged();
            }
        }

        public TROfficeRecordEntry TROffice
        {
            get { return trOffice; }
            set
            {
                trOffice = value;
                OnPropertyChanged();
            }
        }

        public InvigilateConstraint(DateTime from, DateTime to, TROfficeRecordEntry excludeTROffice)
        {
            if (from > to)
                throw new ArgumentOutOfRangeException("to", to.ToString());
            From = from;
            To = to;
            TROffice = excludeTROffice ?? throw new ArgumentNullException(nameof(excludeTROffice));
        }
        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region IEquatable members and equality comparsion methods
        public override bool Equals(object obj)
        {
            return obj is InvigilateConstraint constraint && Equals(constraint);
        }

        public bool Equals([AllowNull] InvigilateConstraint other)
        {
            return other is InvigilateConstraint constraint &&
                   From == constraint.From &&
                   To == constraint.To &&
                   EqualityComparer<TROfficeRecordEntry>.Default.Equals(TROffice, constraint.TROffice);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To, TROffice);
        }
        #endregion

        #region IEditableObject members
        private InvigilateConstraint backup;
        private bool inTx = false;

        public void BeginEdit()
        {
            if (!inTx)
            {
                backup = new InvigilateConstraint(From, To, TROffice);
                inTx = true;
            }
        }

        public void CancelEdit()
        {
            if (inTx)
            {
                From = backup.From;
                To = backup.To;
                TROffice = backup.TROffice;
                backup = null;
                inTx = false;
            }
        }

        public void EndEdit()
        {
            if (inTx)
            {
                backup = null;
                inTx = false;
            }
        }
        #endregion
    }
}
