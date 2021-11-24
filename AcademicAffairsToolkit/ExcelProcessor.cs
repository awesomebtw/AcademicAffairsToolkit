using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ganss.Excel;

namespace AcademicAffairsToolkit
{
    static class ExcelProcessor
    {
        private static (TimeSpan, TimeSpan) ParseTimeIntervalString(string timeString)
        {
            var timeMatch = Regex.Match(timeString, @"(\d+:\d+)-(\d+:\d+)");
            if (timeMatch.Groups.Count >= 2)
                return (TimeSpan.Parse(timeMatch.Groups[1].Value), TimeSpan.Parse(timeMatch.Groups[2].Value));
            else
                throw new FormatException("time interval string format is incorrect");
        }

        public static async Task<IEnumerable<InvigilateRecordEntry>> ReadInvigilateTableAsync(string path, FileParsePolicy policy)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var mapper = new ExcelMapper()
            {
                TrackObjects = false,
                HeaderRowNumber = policy.HeaderRow,
                MinRowNumber = policy.StartRow
            };

            static object SetDateProp(object p)
            {
                return p switch
                {
                    DateTime date => date,
                    double d => DateTime.FromOADate(d),
                    string s when DateTime.TryParse(s, out var date) => date,
                    _ => throw new InvalidOperationException(nameof(p))
                };
            }

            mapper.AddMapping<InvigilateRecordEntry>(Resource.DateColumn, p => p.StartTime).FromExcelOnly()
                .SetPropertyUsing(SetDateProp);
            mapper.AddMapping<InvigilateRecordEntry>(Resource.DateColumn, p => p.EndTime).FromExcelOnly()
                .SetPropertyUsing(SetDateProp);
            mapper.AddMapping<InvigilateRecordEntry>(Resource.TimeIntervalColumn, p => p.StartTime).FromExcelOnly()
                .SetPropertyUsing<InvigilateRecordEntry>((entry, v, c) =>
                {
                    // use start time column here to add
                    (var start, var end) = ParseTimeIntervalString(v as string);
                    entry.StartTime = entry.StartTime.Date.Add(start);
                    entry.EndTime = entry.EndTime.Date.Add(end);
                    return entry.StartTime;
                });

            return await mapper.FetchAsync<InvigilateRecordEntry>(stream, policy.Sheet);
        }

        public static async Task<IEnumerable<TROfficeRecordEntry>> ReadTROfficeTableAsync(string path, FileParsePolicy policy)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var mapper = new ExcelMapper()
            {
                TrackObjects = false,
                HeaderRowNumber = policy.HeaderRow,
                MinRowNumber = policy.StartRow
            };

            return await mapper.FetchAsync<TROfficeRecordEntry>(stream, policy.Sheet);
        }

        public static async Task SaveFileAsync(IEnumerable<ArrangementResultEntry[]> results, string path, bool xls)
        {
            var mapper = new ExcelMapper();

            for (int i = 0; i < results.Count(); i++)
            {
                await mapper.SaveAsync(path, results.ElementAt(i), i, !xls);
            }
        }
    }
}
