using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.UserModel;

namespace AcademicAffairsToolkit
{
    class InvigilateRecordEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Subject { get; set; }
        public string Department { get; set; }
        public int Grade { get; set; }
        public int ExamineeCount { get; set; }
        public string Location { get; set; }
    }

    class InvigilateFileParsePolicy
    {
        public int Sheet { get; set; }
        public int DateColumn { get; set; }
        public int TimeIntervalColumn { get; set; }
        public int SubjectColumn { get; set; }
        public int DepartmentColumn { get; set; }
        public int GradeColumn { get; set; }
        public int SpecialityColumn { get; set; }
        public int ExamineeCountColumn { get; set; }
        public int LocationColumn { get; set; }

        public static readonly InvigilateFileParsePolicy DefaultPolicy = new InvigilateFileParsePolicy
        {
            Sheet = 0,
            DateColumn = 0,
            TimeIntervalColumn = 1,
            SubjectColumn = 2,
            DepartmentColumn = 3,
            GradeColumn = 4,
            SpecialityColumn = 5,
            ExamineeCountColumn = 6,
            LocationColumn = 7
        };
    }

    static class ExcelProcessor
    {
        static IEnumerable<InvigilateRecordEntry> Read(string path, string password, InvigilateFileParsePolicy policy)
        {
            var workbook = WorkbookFactory.Create(path, password, true);
            var sheet = workbook.GetSheetAt(policy.Sheet);

            foreach (IRow row in sheet)
            {
                string dateString = row.GetCell(policy.DateColumn).StringCellValue;
                string[] timeStrings = row.GetCell(policy.TimeIntervalColumn).StringCellValue.Split('-');

                int.TryParse(row.GetCell(policy.ExamineeCountColumn).StringCellValue, out int examineeCount);

                yield return new InvigilateRecordEntry
                {
                    Department = row.GetCell(policy.DepartmentColumn).StringCellValue,
                    Subject = row.GetCell(policy.SubjectColumn).StringCellValue,
                    StartTime = DateTime.Parse($"{dateString} {timeStrings[0]}"),
                    EndTime = DateTime.Parse($"{dateString} {timeStrings[1]}"),
                    Location = row.GetCell(policy.LocationColumn).StringCellValue,
                    ExamineeCount = examineeCount
                };
            }
        }
    }
}
