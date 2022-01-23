using System;
using System.Collections.Generic;

namespace Druzhbank.Models
{
    public class PaginatedListModel<T>
    {
        public List<T>? data { get; set; }
        public int currentPage  { get; set; }
        public Boolean isNext  { get; set; }
    }
}