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
}
