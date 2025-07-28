using ITexAPI.Models.DTOs;

namespace ITexAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> GetByEmailAsync(string email);
        Task<UserDto> UpdateUserAsync(int id, UserDto dto);
        Task DisableUserAsync(int id);
        Task EnableUserAsync(int id);
        Task DeleteUserAsync(int id);
    }
}
