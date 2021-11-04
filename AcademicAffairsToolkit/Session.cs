using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcademicAffairsToolkit
{
    static class Session
    {
        public static InvigilateFileParsePolicy InvigilateFilePolicy = InvigilateFileParsePolicy.DefaultPolicy;

        public static TROfficeFileParsePolicy TROfficeFilePolicy = TROfficeFileParsePolicy.DefaultPolicy;

        public static ObservableCollection<InvigilateRecordEntry> InvigilateRecords = null;

        public static ObservableCollection<TROfficeRecordEntry> TROffices = null;

        // todo: store arrangement data
        public static IEnumerable<object> Arrangements;

        public static bool CanStartArrange() =>
            InvigilateRecords != null && InvigilateRecords.Count != 0 &&
            TROffices != null && TROffices.Count != 0;
    }
}
