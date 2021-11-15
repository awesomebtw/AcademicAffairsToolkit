using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AcademicAffairsToolkit
{
    class InvigilateConstraint : IEquatable<InvigilateConstraint>, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
    }
}
