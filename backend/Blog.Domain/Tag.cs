using System.ComponentModel.DataAnnotations;

namespace Blog.Domain
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}