using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId);
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
    }
}