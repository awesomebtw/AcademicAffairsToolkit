using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AcademicAffairsToolkit
{
    static class Session
    {
        public static ObservableCollection<InvigilateRecordEntry> InvigilateRecords;

        // todo: store arrangement data
        public static IEnumerable<object> Arrangements;
    }
}
