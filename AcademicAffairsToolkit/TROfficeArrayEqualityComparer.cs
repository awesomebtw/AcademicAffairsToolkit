using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// equality comparer for arrangement algorithm
    /// </summary>
    class TROfficeArrayEqualityComparer : IEqualityComparer<TROfficeRecordEntry[]>
    {
        public bool Equals([AllowNull] TROfficeRecordEntry[] x, [AllowNull] TROfficeRecordEntry[] y)
        {
            return x?.SequenceEqual(y) ?? y == null;
        }

        public int GetHashCode([DisallowNull] TROfficeRecordEntry[] obj)
        {
            HashCode hashCode = new HashCode();
            foreach (var x in obj)
                hashCode.Add(x);
            return HashCode.Combine(obj);
        }
    }
}
