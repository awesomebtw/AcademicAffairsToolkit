using IntervalTree;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace AcademicAffairsToolkit
{
    class InvigilateConstraint : IEquatable<InvigilateConstraint>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TROfficeRecordEntry TROffice { get; set; }

        public InvigilateConstraint(DateTime from, DateTime to, TROfficeRecordEntry excludeTROffice)
        {
            From = from;
            To = to;
            TROffice = excludeTROffice ?? throw new ArgumentNullException(nameof(excludeTROffice));
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
