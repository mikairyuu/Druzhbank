using Druzhbank.Enums;

namespace Druzhbank.Models
{
    public class TranslationModel
    {
        public string? token { get; set; }
        public string? sourse { get; set; }
        public string? dest { get; set; }
        public double? sum { get; set; }
        
        public PayType payType { get; set; }
    }
}