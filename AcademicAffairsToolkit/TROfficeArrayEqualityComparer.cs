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
            return x?.SequenceEqual(y) ?? false;
        }

        public int GetHashCode([DisallowNull] TROfficeRecordEntry[] obj)
        {
            return obj.GetHashCode();
        }
    }
}
