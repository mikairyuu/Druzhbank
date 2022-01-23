using Druzhbank.Enums;

namespace Druzhbank.Models
{
    public class TranslationModel
    {
        public string? token { get; set; }
        public string? source { get; set; }
        public string? dest { get; set; }
        public decimal? sum { get; set; }

        public PayType payType { get; set; }
        
        
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