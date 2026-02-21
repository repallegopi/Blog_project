namespace Blog.Domain
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "User";

        // Approval System
        public bool IsApproved { get; set; } = true;

        // Writer Request System
        public bool IsWriterRequestPending { get; set; } = false;
        public DateTime? WriterRequestedAt { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}