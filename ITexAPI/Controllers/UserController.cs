using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITexAPI.Models.DTOs;
using ITexAPI.Services.Interfaces;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Protect as needed
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> Get(int id)
            => Ok(await _userService.GetByIdAsync(id));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
            => Ok(await _userService.GetAllAsync());

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UserDto dto)
            => Ok(await _userService.UpdateUserAsync(id, dto));

        [HttpPut("{id}/disable")]
        public async Task<ActionResult> Disable(int id)
        {
            await _userService.DisableUserAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/enable")]
        public async Task<ActionResult> Enable(int id)
        {
            await _userService.EnableUserAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
