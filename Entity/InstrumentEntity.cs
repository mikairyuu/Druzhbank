using System;

namespace Druzhbank.Entity
{
    public class InstrumentEntity
    {
        public int? id { get; set; }
        public int? user_id { get; set; }
        public String? name { get; set; }
        public String? number { get; set; }
        public String? count { get; set; }
        public Boolean? is_blocked { get; set; }
        public String? hash_cvv { get; set; }
        public DateTime? payment_date { get; set; }
        public DateTime? expairy_date { get; set; }
    }
}