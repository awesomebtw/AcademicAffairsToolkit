using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcademicAffairsToolkit
{
    static class Session
    {
        public static InvigilateFileParsePolicy FileParsePolicy = InvigilateFileParsePolicy.DefaultPolicy;

        public static ObservableCollection<InvigilateRecordEntry> InvigilateRecords = null;

        // todo: store arrangement data
        public static IEnumerable<object> Arrangements;
    }
}
