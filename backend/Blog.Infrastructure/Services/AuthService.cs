using Blog.Application.Auth;
using Blog.Application.Services;
using Blog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blog.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ================= REGISTER =================
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                throw new Exception("Email already exists.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            if (request.Role == "BlogWriter")
            {
                user.Role = "BlogWriter";
                user.IsApproved = false;
            }
            else
            {
                user.Role = "User";
                user.IsApproved = true;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Role == "BlogWriter" && !user.IsApproved)
            {
                return new AuthResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Token = null
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }

        // ================= LOGIN =================
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user == null)
                throw new Exception("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials.");

            if (user.Role == "BlogWriter" && !user.IsApproved)
                throw new Exception("Writer account not approved yet.");

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }

        // ================= JWT =================
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}