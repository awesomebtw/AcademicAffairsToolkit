using Ganss.Excel;
using System;
using System.Collections.Generic;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// a class representing an invigilate task entry
    /// </summary>
    class InvigilateRecordEntry : IEquatable<InvigilateRecordEntry>
    {
        [Column("开始时间", MappingDirections.ObjectToExcel)]
        public DateTime StartTime { get; set; }

        [Column("结束时间", MappingDirections.ObjectToExcel)]
        public DateTime EndTime { get; set; }

        [Column("考试科目")]
        public string Subject { get; set; }

        [Column("学院")]
        public string Department { get; set; }

        [Column("年级")]
        public string Grade { get; set; }

        [Column("专业")]
        public string Specialty { get; set; }

        [Column("人数")]
        public int ExamineeCount { get; set; }

        [Column("考试地点")]
        public string Location { get; set; }

        [Column("考试方式")]
        public string ExamAspect { get; set; }

        public override string ToString()
        {
            // trim location string
            int pos = Location.IndexOfAny(new char[] { ' ', '\n', '\t' });
            if (pos == -1)
                pos = Location.Length - 1;
            return $"{Subject}[{Location[..pos]}, {StartTime.ToShortDateString()} {StartTime.ToShortTimeString()}~{EndTime.ToShortTimeString()}]";
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
                   Specialty == other.Specialty &&
                   ExamineeCount == other.ExamineeCount &&
                   Location == other.Location &&
                   ExamAspect == other.ExamAspect;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(StartTime);
            hash.Add(EndTime);
            hash.Add(Subject);
            hash.Add(Department);
            hash.Add(Grade);
            hash.Add(Specialty);
            hash.Add(ExamineeCount);
            hash.Add(Location);
            hash.Add(ExamAspect);
            return hash.ToHashCode();
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
