using System;

namespace Druzhbank.Entity
{
    public class HistotyItemEntity
    {
        public int? id { get; set; }
        public int type { get; set; }
        public int? instrument_type { get; set; }
        public String? count { get; set; }
        public DateTime? date { get; set; }
        public String? dest { get; set; }
        public String? source { get; set; }
        public String? date_string { get; set; }
    }
}