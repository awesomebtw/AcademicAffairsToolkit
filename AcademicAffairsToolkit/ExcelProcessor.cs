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

    class TROfficeRecordEntry
    {
        public string Name { get; set; }

        public int PeopleCount { get; set; }

        public string Director { get; set; }
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

    class TROfficeFileParsePolicy
    {
        public int Sheet { get; set; }

        public int NameColumn { get; set; }

        public int PeopleCountColumn { get; set; }

        public int DirectorColumn { get; set; }

        public int StartRow { get; set; }

        public static readonly TROfficeFileParsePolicy DefaultPolicy = new TROfficeFileParsePolicy
        {
            Sheet = 0,
            NameColumn = 0,
            PeopleCountColumn = 1,
            DirectorColumn = 2,
            StartRow = 1
        };
    }

    static class ExcelProcessor
    {
        private static int ExtractIntFromDynamicTypedCell(ICell cell, int defaultValue = 0)
        {
            if (cell.CellType == CellType.Numeric)
                return (int)cell.NumericCellValue;
            else if (cell.CellType == CellType.String &&
                int.TryParse(cell.StringCellValue, out int result))
                return result;
            else
                return defaultValue;
        }

        private static DateTime ExtractDateTimeFromDynamicTypedCell(ICell cell, DateTime defaultValue = default)
        {
            if (cell.CellType == CellType.Numeric)
                return cell.DateCellValue;
            else if (cell.CellType == CellType.String &&
                DateTime.TryParse(cell.StringCellValue, out DateTime result))
                return result;
            else
                return defaultValue;
        }

        public static IEnumerable<InvigilateRecordEntry> ReadInvigilateTable(string path, string password, InvigilateFileParsePolicy policy)
        {
            WorkbookFactory.SetImportOption(ImportOption.SheetContentOnly);
            var workbook = WorkbookFactory.Create(path, password, true);
            var sheet = workbook.GetSheetAt(policy.Sheet);

            for (int i = policy.StartRow; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                // working with date and time
                // todo: validate date and time
                DateTime date = ExtractDateTimeFromDynamicTypedCell(row.GetCell(policy.DateColumn));
                
                string timeString = row.GetCell(policy.TimeIntervalColumn).StringCellValue;
                var timeMatch = Regex.Match(timeString, @"(\d+:\d+)-(\d+:\d+)");

                // working with examinee count
                int examineeCount = ExtractIntFromDynamicTypedCell(row.GetCell(policy.ExamineeCountColumn));

                // working with grade
                int grade = ExtractIntFromDynamicTypedCell(row.GetCell(policy.GradeColumn));

                yield return new InvigilateRecordEntry
                {
                    Department = row.GetCell(policy.DepartmentColumn).StringCellValue.Trim(),
                    Subject = row.GetCell(policy.SubjectColumn).StringCellValue.Trim(),
                    StartTime = date.Add(TimeSpan.Parse(timeMatch.Groups[1].Value)),
                    EndTime = date.Add(TimeSpan.Parse(timeMatch.Groups[2].Value)),
                    Grade = grade,
                    Location = row.GetCell(policy.LocationColumn).StringCellValue.Trim(),
                    ExamineeCount = examineeCount
                };
            }
        }

        public static IEnumerable<TROfficeRecordEntry> ReadTROfficeTable(string path, string password, TROfficeFileParsePolicy policy)
        {
            WorkbookFactory.SetImportOption(ImportOption.SheetContentOnly);
            var workbook = WorkbookFactory.Create(path, password, true);
            var sheet = workbook.GetSheetAt(policy.Sheet);

            for (int i = policy.StartRow; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                // working with number of people
                int peopleCount = ExtractIntFromDynamicTypedCell(row.GetCell(policy.PeopleCountColumn));

                yield return new TROfficeRecordEntry
                {
                    Name = row.GetCell(policy.NameColumn).StringCellValue,
                    PeopleCount = peopleCount,
                    Director = row.GetCell(policy.DirectorColumn).StringCellValue
                };
            }
        }
    }
}
