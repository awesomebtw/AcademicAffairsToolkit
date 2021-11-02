using System.Collections.Generic;

namespace AcademicAffairsToolkit
{
    enum SessionStatus
    {
        FileNotOpen,
        FileOpened,
        Evaluated,
        Invalid
    }

    class Session
    {
        public SessionStatus Status { get; set; }
        public IEnumerable<InvigilateRecordEntry> InvigilateRecords { get; set; }
    }
}
