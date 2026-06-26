using System;
using System.Threading.Tasks;
using TaskManager.API.DTOs.Auth;
using TaskManager.API.Models;

namespace TaskManager.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<AppUser?> GetCurrentUserAsync(Guid userId);
    }
}
