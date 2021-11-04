using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        public int StartRow { get; set; }

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
            LocationColumn = 7,
            StartRow = 2
        };
    }

    static class ExcelProcessor
    {
        public static IEnumerable<InvigilateRecordEntry> Read(string path, string password, InvigilateFileParsePolicy policy)
        {
            var workbook = WorkbookFactory.Create(path, password, true);
            var sheet = workbook.GetSheetAt(policy.Sheet);

            for (int i = policy.StartRow; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                // working with date and time
                ICell dateCell = row.GetCell(policy.DateColumn);
                DateTime date = dateCell.CellType == CellType.Numeric ? dateCell.DateCellValue : DateTime.Parse(dateCell.StringCellValue);
                string timeString = row.GetCell(policy.TimeIntervalColumn).StringCellValue;
                var timeMatch = Regex.Match(timeString, @"(\d+:\d+)-(\d+:\d+)");

                // working with examinee count
                var examineeCountCell = row.GetCell(policy.ExamineeCountColumn);
                int examineeCount;
                if (examineeCountCell.CellType == CellType.Numeric)
                    examineeCount = (int)examineeCountCell.NumericCellValue;
                else
                    int.TryParse(examineeCountCell.StringCellValue, out examineeCount);

                yield return new InvigilateRecordEntry
                {
                    Department = row.GetCell(policy.DepartmentColumn).StringCellValue,
                    Subject = row.GetCell(policy.SubjectColumn).StringCellValue,
                    StartTime = date.Add(TimeSpan.Parse(timeMatch.Groups[1].Value)),
                    EndTime = date.Add(TimeSpan.Parse(timeMatch.Groups[2].Value)),
                    Location = row.GetCell(policy.LocationColumn).StringCellValue,
                    ExamineeCount = examineeCount
                };
            }
        }
    }
}
