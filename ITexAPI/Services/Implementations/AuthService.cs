using AutoMapper;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Models.Entities;
using ITexAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ITexAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
                if (userExists != null)
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Email is already registered.");

                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Registration failed.",
                        result.Errors.Select(e => e.Description).ToList());

                var (token, expiresAt) = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);

                return ApiResponse<AuthResponseDto>.SuccessResponse(new AuthResponseDto
                {
                    Token = token,
                    User = userDto,
                    ExpiresAt = expiresAt
                }, "User registered successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.ErrorResponse($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null || !user.IsActive)
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or user is disabled.");

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded)
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid credentials.");

                var (token, expiresAt) = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);

                return ApiResponse<AuthResponseDto>.SuccessResponse(new AuthResponseDto
                {
                    Token = token,
                    User = userDto,
                    ExpiresAt = expiresAt
                }, "Login successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.ErrorResponse($"Login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return ApiResponse<UserDto>.ErrorResponse("User not found.");

                var userDto = _mapper.Map<UserDto>(user);
                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
                return ApiResponse<IEnumerable<UserDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<UserDto>>.ErrorResponse($"Error retrieving users: {ex.Message}");
            }
        }

        private (string token, DateTime expiresAt) GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("lastName", user.LastName ?? "")
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440"));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}