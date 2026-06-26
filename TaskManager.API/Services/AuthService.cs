using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs.Auth;
using TaskManager.API.Helpers;
using TaskManager.API.Models;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExists)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());
            if (usernameExists)
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            var user = new AppUser
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);
            return new AuthResponseDto { Token = token };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
            {
                return null;
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null;
            }

            var token = _jwtHelper.GenerateToken(user);
            return new AuthResponseDto { Token = token };
        }

        public async Task<AppUser?> GetCurrentUserAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}
