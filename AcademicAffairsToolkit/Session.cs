using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcademicAffairsToolkit
{
    static class Session
    {
        public static InvigilateFileParsePolicy InvigilateFilePolicy { get; set; } = InvigilateFileParsePolicy.DefaultPolicy;

        public static TROfficeFileParsePolicy TROfficeFilePolicy { get; set; } = TROfficeFileParsePolicy.DefaultPolicy;

        public static ObservableCollection<InvigilateRecordEntry> InvigilateRecords { get; set; }

        public static ObservableCollection<TROfficeRecordEntry> TROffices { get; set; }

        // todo: store arrangement data
        public static IEnumerable<object> Arrangements { get; set; }

        public static bool AnyFileLoaded() =>
            InvigilateRecords != null || TROffices != null;

        public static bool CanStartArrange() =>
            InvigilateRecords != null && InvigilateRecords.Count != 0 &&
            TROffices != null && TROffices.Count != 0;
    }
}
