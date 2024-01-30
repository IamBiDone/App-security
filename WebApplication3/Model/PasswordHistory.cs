namespace WebApplication3.Model
{
    public class PasswordHistory
    {
        public int Id { get; set; } 
        public string UserId { get; set; } 
        public string HashedPassword { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}
