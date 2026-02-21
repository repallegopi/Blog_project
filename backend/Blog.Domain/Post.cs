namespace Blog.Domain
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign Keys
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}