using System;

namespace Druzhbank.Models
{
    public class CheckModel:InstrumentModel
    {
        public Boolean? is_blocked { get; set; }
    }
}