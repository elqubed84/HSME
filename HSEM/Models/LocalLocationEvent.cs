using SQLite;

namespace HSEM.Models
{
    public class LocalLocationEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string EventType { get; set; }  // "خروج عن نطاق العمل" أو "دخول نطاق العمل"
        public double Distance { get; set; }
        public DateTime Time { get; set; }
        public bool IsSynced { get; set; }
    }
}