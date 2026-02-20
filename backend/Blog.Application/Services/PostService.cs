using Blog.Application.Interfaces;
using Blog.Domain;

namespace Blog.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repository;

        public PostService(IPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Post> CreateAsync(Post post)
        {
            await _repository.AddAsync(post);
            return post;
        }

        public async Task UpdateAsync(Post post)
        {
            await _repository.UpdateAsync(post);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}