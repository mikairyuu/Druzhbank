namespace Druzhbank.Responses
{
    public class TokenNumberResponse
    {
        public string? number { get; set; }
        public string? token { get; set; }

        public int operationCount { get; set; }
    }
}