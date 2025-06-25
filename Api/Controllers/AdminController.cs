using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        // ทุกคนเข้าได้ (ไม่มี [Authorize])
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new 
            { 
                Message = "This is a public endpoint - everyone can access",
                Timestamp = DateTime.UtcNow,
                RequiredAuth = "None"
            });
        }

        // ต้องล็อกอินก่อน (มี [Authorize])
        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new 
            { 
                Message = "This endpoint requires authentication",
                UserId = userId,
                Email = email,
                Role = role,
                RequiredAuth = "Any authenticated user"
            });
        }

        // เฉพาะ Admin เท่านั้น
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnlyEndpoint()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new 
            { 
                Message = "This endpoint requires Admin role",
                UserId = userId,
                Email = email,
                Role = role,
                RequiredAuth = "Admin role only",
                SecretData = "Top secret admin information!"
            });
        }

        // เฉพาะ User หรือ Admin
        [HttpGet("user-or-admin")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult UserOrAdminEndpoint()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new 
            { 
                Message = "This endpoint allows User or Admin roles",
                YourRole = role,
                RequiredAuth = "User or Admin role",
                Data = $"Welcome {role}!"
            });
        }

        // Test endpoint สำหรับดูข้อมูลใน JWT Token
        [HttpGet("token-info")]
        [Authorize]
        public IActionResult TokenInfo()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new 
            { 
                Message = "JWT Token Information",
                Claims = claims,
                TotalClaims = claims.Count
            });
        }
    }
}