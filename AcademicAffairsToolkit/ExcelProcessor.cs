using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace AcademicAffairsToolkit
{
    class InvigilateFileParsePolicy
    {
        public int Sheet { get; set; }

        public int DateColumn { get; set; }

        public int TimeIntervalColumn { get; set; }

        public int SubjectColumn { get; set; }

        public int DepartmentColumn { get; set; }

        public int GradeColumn { get; set; }

        public int SpecialtyColumn { get; set; }

        public int ExamineeCountColumn { get; set; }

        public int LocationColumn { get; set; }

        public int ExamAspectColumn { get; set; }

        public int StartRow { get; set; }

        public static readonly InvigilateFileParsePolicy DefaultPolicy = new InvigilateFileParsePolicy
        {
            Sheet = 0,
            DateColumn = 0,
            TimeIntervalColumn = 1,
            SubjectColumn = 2,
            DepartmentColumn = 3,
            GradeColumn = 4,
            SpecialtyColumn = 5,
            ExamineeCountColumn = 6,
            LocationColumn = 7,
            ExamAspectColumn = 8,
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
            return cell.CellType switch
            {
                CellType.Numeric => (int)cell.NumericCellValue,
                CellType.String when int.TryParse(cell.StringCellValue, out int result) => result,
                _ => defaultValue
            };
        }

        private static DateTime ExtractDateTimeFromDynamicTypedCell(ICell cell, DateTime defaultValue = default)
        {
            return cell.CellType switch
            {
                CellType.Numeric => cell.DateCellValue,
                CellType.String when DateTime.TryParse(cell.StringCellValue, out DateTime result) => result,
                _ => defaultValue
            };
        }

        private static (TimeSpan, TimeSpan) ParseTimeIntervalString(string timeString)
        {
            var timeMatch = Regex.Match(timeString, @"(\d+:\d+)-(\d+:\d+)");
            if (timeMatch.Groups.Count >= 2)
                return (TimeSpan.Parse(timeMatch.Groups[1].Value), TimeSpan.Parse(timeMatch.Groups[2].Value));
            else
                throw new InvalidOperationException(nameof(timeString));
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
                DateTime date = ExtractDateTimeFromDynamicTypedCell(row.GetCell(policy.DateColumn));

                (var startTimeInterval, var endTimeInterval) = ParseTimeIntervalString(row.GetCell(policy.TimeIntervalColumn).StringCellValue);

                // working with examinee count
                int examineeCount = ExtractIntFromDynamicTypedCell(row.GetCell(policy.ExamineeCountColumn));

                // working with grade
                int grade = ExtractIntFromDynamicTypedCell(row.GetCell(policy.GradeColumn));

                yield return new InvigilateRecordEntry
                {
                    Department = row.GetCell(policy.DepartmentColumn).StringCellValue.Trim(),
                    Subject = row.GetCell(policy.SubjectColumn).StringCellValue.Trim(),
                    StartTime = date.Add(startTimeInterval),
                    EndTime = date.Add(endTimeInterval),
                    Grade = grade,
                    Location = row.GetCell(policy.LocationColumn).StringCellValue.Trim(),
                    ExamineeCount = examineeCount,
                    ExamAspect = row.GetCell(policy.ExamAspectColumn).StringCellValue.Trim(),
                    Specialty = row.GetCell(policy.SpecialtyColumn).StringCellValue.Trim()
                };
            }
        }

        public static async Task<IEnumerable<InvigilateRecordEntry>> ReadInvigilateTableAsync(string path, string password, InvigilateFileParsePolicy policy)
        {
            return await Task.Run(() => ReadInvigilateTable(path, password, policy));
        }

        public static IEnumerable<TROfficeRecordEntry> ReadTROfficeTable(string path, string password, TROfficeFileParsePolicy policy)
        {
            WorkbookFactory.SetImportOption(ImportOption.SheetContentOnly);
            var workbook = WorkbookFactory.Create(path, password, true);
            var sheet = workbook.GetSheetAt(policy.Sheet);

            for (int i = policy.StartRow; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);

                // working with people count
                int peopleCount = ExtractIntFromDynamicTypedCell(row.GetCell(policy.PeopleCountColumn));

                yield return new TROfficeRecordEntry
                {
                    Name = row.GetCell(policy.NameColumn).StringCellValue.Trim(),
                    PeopleCount = peopleCount,
                    Director = row.GetCell(policy.DirectorColumn).StringCellValue.Trim()
                };
            }
        }

        public static async Task<IEnumerable<TROfficeRecordEntry>> ReadTROfficeTableAsync(string path, string password, TROfficeFileParsePolicy policy)
        {
            return await Task.Run(() => ReadTROfficeTable(path, password, policy));
        }

        public static void SaveFile(IEnumerable<ArrangementResultEntry[]> arrangementResults, string path, bool oldFormat)
        {
            IWorkbook workbook = oldFormat ? new HSSFWorkbook() : (IWorkbook)new XSSFWorkbook();

            IDataFormat format = workbook.CreateDataFormat();
            ICellStyle dateCellStyle = workbook.CreateCellStyle();
            dateCellStyle.DataFormat = format.GetFormat("yyyy/m/d");

            ICellStyle timeCellStyle = workbook.CreateCellStyle();
            timeCellStyle.DataFormat = format.GetFormat("h:mm");

            foreach (var solution in arrangementResults)
            {
                var sheet = workbook.CreateSheet();
                var headerRow = sheet.CreateRow(0);

                headerRow.CreateCell(1).SetCellValue(nameof(InvigilateRecordEntry.StartTime));
                headerRow.CreateCell(2).SetCellValue(nameof(InvigilateRecordEntry.EndTime));
                headerRow.CreateCell(3).SetCellValue(nameof(InvigilateRecordEntry.Subject));
                headerRow.CreateCell(4).SetCellValue(nameof(InvigilateRecordEntry.Department));
                headerRow.CreateCell(5).SetCellValue(nameof(InvigilateRecordEntry.Grade));
                headerRow.CreateCell(6).SetCellValue(nameof(InvigilateRecordEntry.Specialty));
                headerRow.CreateCell(7).SetCellValue(nameof(InvigilateRecordEntry.ExamineeCount));
                headerRow.CreateCell(8).SetCellValue(nameof(InvigilateRecordEntry.Location));
                headerRow.CreateCell(9).SetCellValue(nameof(InvigilateRecordEntry.ExamAspect));
                headerRow.CreateCell(10).SetCellValue(nameof(TROfficeRecordEntry.Name));
                headerRow.CreateCell(11).SetCellValue(nameof(TROfficeRecordEntry.Director));

                for (int i = 0; i < solution.Length; i++)
                {
                    var row = sheet.CreateRow(i + 1);
                    var entry = solution[i];

                    ICell dateCell = row.CreateCell(0);
                    dateCell.SetCellValue(entry.InvigilateRecord.StartTime);
                    dateCell.CellStyle = dateCellStyle;

                    ICell startTimeCell = row.CreateCell(1);
                    startTimeCell.SetCellValue(entry.InvigilateRecord.StartTime);
                    startTimeCell.CellStyle = timeCellStyle;

                    ICell endTimeCell = row.CreateCell(2);
                    endTimeCell.SetCellValue(entry.InvigilateRecord.EndTime);
                    endTimeCell.CellStyle = timeCellStyle;

                    row.CreateCell(3).SetCellValue(entry.InvigilateRecord.Subject);
                    row.CreateCell(4).SetCellValue(entry.InvigilateRecord.Department);
                    row.CreateCell(5).SetCellValue(entry.InvigilateRecord.Grade);
                    row.CreateCell(6).SetCellValue(entry.InvigilateRecord.Specialty);
                    row.CreateCell(7).SetCellValue(entry.InvigilateRecord.ExamineeCount);
                    row.CreateCell(8).SetCellValue(entry.InvigilateRecord.Location);
                    row.CreateCell(9).SetCellValue(entry.InvigilateRecord.ExamAspect);

                    row.CreateCell(10).SetCellValue(entry.TROfficeRecord.Name);
                    row.CreateCell(11).SetCellValue(entry.TROfficeRecord.Director);
                }
            }

            using var stream = new FileStream(path, FileMode.Create);
            workbook.Write(stream);
        }

        public static async Task SaveFileAsync(IEnumerable<ArrangementResultEntry[]> arrangementResults, string path, bool oldFormat)
        {
            await Task.Run(() => SaveFile(arrangementResults, path, oldFormat));
        }
    }
}
