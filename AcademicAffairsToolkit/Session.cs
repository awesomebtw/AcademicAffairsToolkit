using IntervalTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AcademicAffairsToolkit
{
    static class Session
    {
        public static InvigilateFileParsePolicy InvigilateFilePolicy { get; set; } = InvigilateFileParsePolicy.DefaultPolicy;

        public static TROfficeFileParsePolicy TROfficeFilePolicy { get; set; } = TROfficeFileParsePolicy.DefaultPolicy;

        public static IEnumerable<InvigilateRecordEntry> InvigilateRecords { get; set; }

        public static IEnumerable<TROfficeRecordEntry> TROffices { get; set; }

        public static ObservableCollection<InvigilateConstraint> Constraints { get; set; } = new ObservableCollection<InvigilateConstraint>();

        // todo: store arrangement data
        public static IEnumerable<object> Arrangements { get; set; }

        public static bool AnyFileLoaded() =>
            InvigilateRecords != null || TROffices != null;

        public static bool CanStartArrange() =>
            InvigilateRecords != null && InvigilateRecords.Count() != 0 &&
            TROffices != null && TROffices.Count() != 0;

        public static bool AutoArrangementFinished() =>
            Arrangements != null && Arrangements.Count() != 0;
    }
}
