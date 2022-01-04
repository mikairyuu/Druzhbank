using System;
using System.Text.Json.Nodes;

namespace Druzhbank.Models
{
    public class InstrumentHistoryItemModel
    {
        public int? id { get; set; }
        public int? type { get; set; }

        public int? instrument_type { get; set; }
        public String? count { get; set; }
        public String? date { get; set; }
        
        public String? dest { get; set; }
        
        public String? source { get; set; }
    }
}