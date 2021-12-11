using System;

namespace Druzhbank.Models
{
    public class CardModel:InstrumentModel
    {
        public Boolean? is_blocked { get; set; }
        public String? hash_cvv { get; set; }
        public DateTime? expairy_date { get; set; }
    }
}