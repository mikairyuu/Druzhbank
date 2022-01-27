namespace Druzhbank.Responses
{
    public class SetTemplateResponse
    {
        public string? token { get; set; }
        public string? source { get; set; }
        public string? dest { get; set; }
        public string? name { get; set; }
        public int? sum { get; set; }
    }
}