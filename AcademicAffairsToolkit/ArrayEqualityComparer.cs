using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// an type-generic element-wise equality comparer implementation for array using <see cref="HashCode"/>
    /// and <see cref="Enumerable.SequenceEqual{T}(IEnumerable{T}, IEnumerable{T})"/>
    /// </summary>
    class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals([AllowNull] T[] x, [AllowNull] T[] y)
        {
            return x?.SequenceEqual(y) ?? y == null;
        }

        public int GetHashCode([DisallowNull] T[] obj)
        {
            HashCode hashCode = new HashCode();
            foreach (var x in obj)
                hashCode.Add(x);
            return hashCode.ToHashCode();
        }
    }
}
