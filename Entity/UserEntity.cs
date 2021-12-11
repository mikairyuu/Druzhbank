namespace Druzhbank.Entity
{
    public class UserEntity
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public string? hash { get; set; }
        public string? token { get; set; }
        public string? salt { get; set; }
    }
}