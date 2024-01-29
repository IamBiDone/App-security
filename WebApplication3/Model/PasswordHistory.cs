namespace WebApplication3.Model
{
    public class PasswordHistory
    {
        public int Id { get; set; } // Primary key
        public string UserId { get; set; } // Foreign key referencing the user
        public string HashedPassword { get; set; } // Hashed version of the password
        public DateTime CreatedAt { get; set; } // Timestamp when the password was added (optional)
    }
}
