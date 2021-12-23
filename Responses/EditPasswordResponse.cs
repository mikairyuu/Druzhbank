namespace Druzhbank.Responses
{
    public class EditPasswordResponse
    {
        public string? old_password { get; set; }
        public string? new_password { get; set; }
        public string? token { get; set; }
    }
}