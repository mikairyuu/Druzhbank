using Druzhbank.Enums;

namespace Druzhbank.Responses
{
    public class EditeInstrumentNameResponse
    {
        public string? name { get; set; }
        public string? token { get; set; }
        public string? number { get; set; }
         public Instrument instrument { get; set; }
    }
}