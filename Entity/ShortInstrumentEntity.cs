using System;

namespace Druzhbank.Entity
{
    public class ShortInstrumentEntity
    {
        public int? id { get; set; }
        public String? name { get; set; }
        public String? number { get; set; }
        public int instrument_type { get; set; }
    }
}