namespace AcademicAffairsToolkit
{
    class FileParsePolicy
    {
        public int Sheet { get; set; }

        public int HeaderRow { get; set; }

        public int StartRow { get; set; }

        public static readonly FileParsePolicy InvigilateFileDefaultPolicy = new FileParsePolicy
        {
            Sheet = 0,
            HeaderRow = 1,
            StartRow = 2
        };

        public static readonly FileParsePolicy TROfficeFileDefaultPolicy = new FileParsePolicy
        {
            Sheet = 0,
            HeaderRow = 0,
            StartRow = 1
        };
    }
}
