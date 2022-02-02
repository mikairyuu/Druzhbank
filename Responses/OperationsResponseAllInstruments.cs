﻿namespace Druzhbank.Responses
{
    public class OperationsResponseAllInstruments
    {
        public string? token { get; set; }
        
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
        
        public string? FindByString { get; set; }
    }
}