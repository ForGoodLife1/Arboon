using Arboon.Application.DTOs.Auth;
using Arboon.Application.Interfaces;
using Arboon.Domain.Entities;
using Arboon.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arboon.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IWalletService _walletService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAppDbContext context,
        IJwtService jwtService,
        IWalletService walletService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());

        if (existingUser != null)
            throw new DuplicateEmailException(dto.Email);

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create wallet for new user
        await _walletService.EnsureWalletExistsAsync(user.Id);

        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation("User {UserId} registered with email {Email}", user.Id, user.Email);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant())
            ?? throw new UnauthorizedException("بيانات الدخول غير صحيحة");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("بيانات الدخول غير صحيحة");

        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation("User {UserId} logged in", user.Id);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
