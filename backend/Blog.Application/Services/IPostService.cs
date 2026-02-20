using Blog.Domain;

namespace Blog.Application.Services
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task<Post> CreateAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(int id);
    }
}