using System;
using System.Text.Json.Nodes;

namespace Druzhbank.Models
{
    public class BankomatEntity
    {
        public int? id { get; set; }
        public string? adress { get; set; }
        public Boolean? is_working { get; set; }
        public Boolean? is_atm { get; set; }
        public TimeSpan? time_start { get; set; }
        public TimeSpan? time_end { get; set; }
        public String? coordinates { get; set; }
    }
}