using System;
using System.Text.Json.Nodes;

namespace Druzhbank.Entity
{
    public class ATMEntity
    {
        public int? id { get; set; }
        public string? adress { get; set; }
        public Boolean? is_working { get; set; }
        public Boolean? is_atm { get; set; }
        public DateTime? time_start { get; set; }
        public DateTime? time_end { get; set; }
        public JsonArray? coordinates { get; set; }
    }
}