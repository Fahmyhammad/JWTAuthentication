namespace WebAPI.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool Enabled { get; set; }
        public string Email { get; set; }
    }
}
