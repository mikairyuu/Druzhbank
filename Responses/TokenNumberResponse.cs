﻿namespace Druzhbank.Responses
{
    public class TokenNumberResponse
    {
        public string? number { get; set; }
        public string? token { get; set; }

        public int operationCount { get; set; }
        
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

    }
}