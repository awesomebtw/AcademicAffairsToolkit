using System;
using System.Collections.Generic;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// a class representing an invigilate task entry
    /// </summary>
    class InvigilateRecordEntry : IEquatable<InvigilateRecordEntry>
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Subject { get; set; }

        public string Department { get; set; }

        public int Grade { get; set; }

        public int ExamineeCount { get; set; }

        public string Location { get; set; }

        public override string ToString()
        {
            return $"{Subject}[{Location}, {StartTime.ToShortDateString()} {StartTime.ToShortTimeString()}~{EndTime.ToShortTimeString()}]";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InvigilateRecordEntry);
        }

        public bool Equals(InvigilateRecordEntry other)
        {
            return other != null &&
                   StartTime == other.StartTime &&
                   EndTime == other.EndTime &&
                   Subject == other.Subject &&
                   Department == other.Department &&
                   Grade == other.Grade &&
                   ExamineeCount == other.ExamineeCount &&
                   Location == other.Location;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartTime, EndTime, Subject, Department, Grade, ExamineeCount, Location);
        }

        public static bool operator ==(InvigilateRecordEntry left, InvigilateRecordEntry right)
        {
            return EqualityComparer<InvigilateRecordEntry>.Default.Equals(left, right);
        }

        public static bool operator !=(InvigilateRecordEntry left, InvigilateRecordEntry right)
        {
            return !(left == right);
        }
    }
}
