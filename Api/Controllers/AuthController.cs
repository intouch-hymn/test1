using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Api.Services;
using Api.Data;
using Api.Models;

namespace Api.Controllers
{
    // API Controller that handles authentication (login, register, profile)
    [ApiController]
    [Route("api/[controller]")] // This creates routes like /api/auth/login
    public class AuthController : ControllerBase
    {
        // Dependency injection - these services are provided by the DI container
        private readonly IJwtService _jwtService;           // Creates JWT tokens
        private readonly IPasswordService _passwordService; // Hashes passwords
        private readonly AppDbContext _context;             // Database access

        // Constructor - DI container automatically provides these services
        public AuthController(IJwtService jwtService, IPasswordService passwordService, AppDbContext context)
        {
            _jwtService = jwtService;
            _passwordService = passwordService;
            _context = context;
        }

        // POST /api/auth/register - Creates a new user account
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Check if someone already registered with this email
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { Message = "อีเมลนี้มีอยู่ในระบบแล้ว" });
            }

            // Create new user object
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                Role = "User", // Default role for new registrations
                // IMPORTANT: Hash the password before storing it!
                PasswordHash = _passwordService.HashPassword(request.Password)
            };

            // Save user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create JWT token for the new user (auto-login after registration)
            // ⭐ Updated: Now includes user.Role parameter
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

            // Return success response with token
            return Ok(new LoginResponse 
            { 
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id,
                Role = user.Role, // Include role in response
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // POST /api/auth/register-admin - Creates a new admin account (restricted)
        [HttpPost("register-admin")]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can create Admin accounts
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
        {
            // Check if someone already registered with this email
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { Message = "อีเมลนี้มีอยู่ในระบบแล้ว" });
            }

            // Create new admin user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                Role = "Admin", // Set role as Admin
                PasswordHash = _passwordService.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create JWT token with Admin role
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

            return Ok(new LoginResponse 
            { 
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // POST /api/auth/login - Authenticates a user and returns a token
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Find user by email in database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            // Check if user exists AND password is correct
            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "อีเมลหรือรหัสผ่านไม่ถูกต้อง" });
            }

            // Create JWT token for authenticated user
            // ⭐ Updated: Now includes user.Role parameter
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);
            
            // Return success response with token
            return Ok(new LoginResponse 
            { 
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id,
                Role = user.Role, // Include role in response
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // GET /api/auth/profile - Gets current user's profile information
        // [Authorize] means this endpoint requires a valid JWT token
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // Extract user ID from JWT token claims
            // The JWT middleware automatically populates User.Claims when token is valid
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value; // Get role from token
            
            // Validate that we got a user ID and it's a valid number
            if (userId == null || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            // Find user in database
            var user = await _context.Users.FindAsync(userIdInt);
            
            if (user == null)
            {
                return NotFound(new { Message = "ไม่พบผู้ใช้" });
            }

            // Return user profile (without password!)
            return Ok(new 
            { 
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                Role = user.Role, // Include role in response
                CreatedAt = user.CreatedAt,
                Message = "ดึงข้อมูลโปรไฟล์สำเร็จ"
            });
        }

        // GET /api/auth/test - Simple test endpoint (no authentication required)
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { Message = "API is working", Timestamp = DateTime.UtcNow });
        }

        // GET /api/auth/user-info - Get basic info about current user from token
        [HttpGet("user-info")]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            // Extract information directly from JWT token (no database query needed)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new 
            { 
                UserId = userId,
                Email = email,
                Role = role,
                Message = "ข้อมูลจาก JWT Token",
                Note = "ข้อมูลนี้มาจากโทเค็นโดยตรง ไม่ต้องเรียกฐานข้อมูล"
            });
        }
    }

    // === DATA TRANSFER OBJECTS (DTOs) ===
    // These define the structure of data sent to/from the API

    // Data required to register a new user
    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)] // Password must be 6-100 characters
        public string Password { get; set; } = string.Empty;
        
        [Range(1, 150)]
        public int Age { get; set; }
    }

    // Data required to login
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // Data returned after successful login/registration
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;        // JWT token
        public string Email { get; set; } = string.Empty;        // User's email
        public string Name { get; set; } = string.Empty;         // User's name
        public int UserId { get; set; }                          // User's ID
        public string Role { get; set; } = string.Empty;         // User's role (NEW)
        public DateTime ExpiresAt { get; set; }                  // When token expires
    }
}