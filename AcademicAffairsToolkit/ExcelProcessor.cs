using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.UserModel;

namespace AcademicAffairsToolkit
{
    static class ExcelProcessor
    {
        static void Read(string path, string password = "")
        {
            // todo: read according to policy
            var workbook = WorkbookFactory.Create(path, password, true);
        }
    }

    class FileParsePolicy
    {
        public int DateColumn { get; set; }
        public int TimeColumn { get; set; }
        public int SubjectColumn { get; set; }
        public int InvigilatorColumn { get; set; }
        public int ExamineeCountColumn { get; set; }
        public int CourseNameColumn { get; set; }
        public string InvigilatorRegex { get; set; }
    }
}
