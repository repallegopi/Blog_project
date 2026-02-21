namespace Blog.Domain
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign Keys
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}