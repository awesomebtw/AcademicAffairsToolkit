using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AcademicAffairsToolkit
{
    static class Session
    {
        public static FileParsePolicy InvigilateFilePolicy { get; set; } = FileParsePolicy.InvigilateFileDefaultPolicy;

        public static FileParsePolicy TROfficeFilePolicy { get; set; } = FileParsePolicy.TROfficeFileDefaultPolicy;

        public static IEnumerable<InvigilateRecordEntry> InvigilateRecords { get; set; }

        public static IEnumerable<TROfficeRecordEntry> TROffices { get; set; }

        public static ObservableCollection<InvigilateConstraint> Constraints { get; set; } = new ObservableCollection<InvigilateConstraint>();

        public static ObservableCollection<ArrangementResultEntry[]> Arrangements { get; set; } = new ObservableCollection<ArrangementResultEntry[]>();

        public static bool AnyFileLoaded() =>
            InvigilateRecords != null || TROffices != null;

        public static bool CanStartArrange() =>
            InvigilateRecords != null && InvigilateRecords.Count() != 0 &&
            TROffices != null && TROffices.Count() != 0;

        public static bool AutoArrangementFinished() =>
            Arrangements != null && Arrangements.Count() != 0;
    }
}
