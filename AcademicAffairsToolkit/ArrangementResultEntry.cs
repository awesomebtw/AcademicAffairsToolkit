using Ganss.Excel;

namespace AcademicAffairsToolkit
{
    class ArrangementResultEntry
    {
        public InvigilateRecordEntry InvigilateRecord { get; set; }

        public TROfficeRecordEntry TROfficeRecord { get; set; }

        [Column("监考人数")]
        public int PeopleNeeded { get; set; }

        public ArrangementResultEntry(InvigilateRecordEntry invigilateRecord, TROfficeRecordEntry trOfficeRecord, int peopleNeeded)
        {
            InvigilateRecord = invigilateRecord;
            TROfficeRecord = trOfficeRecord;
            PeopleNeeded = peopleNeeded;
        }
    }
}
