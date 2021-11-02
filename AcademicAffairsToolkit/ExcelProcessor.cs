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
        public int TimeColumn { get; set; }
        public int DepartmentColumn { get; set; }
        public int SubjectColumn { get; set; }
        public int ExamineeCountColumn { get; set; }
        public int LocationColumn { get; set; }
    }

    static class ExcelProcessor
    {
        static IEnumerable<InvigilateRecordEntry> Read(string path, string password, InvigilateFileParsePolicy policy)
        {
            var workbook = WorkbookFactory.Create(path, password, true);
            var sheet = workbook.GetSheetAt(policy.Sheet);

            foreach (IRow row in sheet)
            {
                // todo: complete row-wise parsing
                string dateString = row.GetCell(policy.DateColumn).StringCellValue;
                string startTimeString = row.GetCell(policy.TimeColumn).StringCellValue;
                string endTimeString = row.GetCell(policy.TimeColumn).StringCellValue;
                _ = int.TryParse(row.GetCell(policy.ExamineeCountColumn).StringCellValue, out int examineeCount);

                yield return new InvigilateRecordEntry
                {
                    Department = row.GetCell(policy.DepartmentColumn).StringCellValue,
                    Subject = row.GetCell(policy.SubjectColumn).StringCellValue,
                    StartTime = DateTime.Parse($"{dateString} {startTimeString}"),
                    EndTime = DateTime.Parse($"{dateString} {endTimeString}"),
                    Location = row.GetCell(policy.LocationColumn).StringCellValue,
                    ExamineeCount = examineeCount
                };
            }
        }
    }
}
