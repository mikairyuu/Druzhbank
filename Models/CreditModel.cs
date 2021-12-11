using System;

namespace Druzhbank.Models
{
    public class CreditModel:InstrumentModel
    {
        public DateTime? payment_date { get; set; }
    }
}