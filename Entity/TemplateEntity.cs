using System;

namespace Druzhbank.Entity
{
    public class TemplateEntity
    {
        public int? id { get; set; }
        public String? name { get; set; }
        public String? source { get; set; }
        public String? dest { get; set; }
        public int? source_type { get; set; }
        public int? dest_type { get; set; }
        public int sum { get; set; }
    }
}