using Ganss.Excel;
using System;
using System.Collections.Generic;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// a class representing a teaching and researching office
    /// </summary>
    class TROfficeRecordEntry : IEquatable<TROfficeRecordEntry>
    {
        [Column("教研室")]
        public string Name { get; set; }

        [Column("人数", MappingDirections.ExcelToObject)]
        public int PeopleCount { get; set; }

        [Column("主任")]
        public string Director { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as TROfficeRecordEntry);
        }

        public bool Equals(TROfficeRecordEntry other)
        {
            return other != null &&
                   Name == other.Name &&
                   PeopleCount == other.PeopleCount &&
                   Director == other.Director;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, PeopleCount, Director);
        }

        public override string ToString()
        {
            return $"{Name} ({Director}, {PeopleCount})";
        }

        public static bool operator ==(TROfficeRecordEntry left, TROfficeRecordEntry right)
        {
            return EqualityComparer<TROfficeRecordEntry>.Default.Equals(left, right);
        }

        public static bool operator !=(TROfficeRecordEntry left, TROfficeRecordEntry right)
        {
            return !(left == right);
        }
    }
}
