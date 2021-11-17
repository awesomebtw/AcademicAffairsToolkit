using System;
using System.Collections.Generic;
using System.Text;

namespace AcademicAffairsToolkit
{
    class ArrangementResultEntry
    {
        public InvigilateRecordEntry InvigilateRecord { get; set; }

        public TROfficeRecordEntry TROfficeRecord { get; set; }

        public int PeopleNeeded { get; set; }

        public ArrangementResultEntry(InvigilateRecordEntry invigilateRecord, TROfficeRecordEntry trOfficeRecord, int peopleNeeded)
        {
            InvigilateRecord = invigilateRecord;
            TROfficeRecord = trOfficeRecord;
            PeopleNeeded = peopleNeeded;
        }
    }
}
