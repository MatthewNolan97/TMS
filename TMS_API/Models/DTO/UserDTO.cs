namespace TMS_API.Models.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string UserType { get; set; } = null!;
        public string? Oid { get; set; }
    }
}
