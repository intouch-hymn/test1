using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using System.ComponentModel.DataAnnotations;  // ← เพิ่มบรรทัดนี้

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(new { data = users, count = users.Count });
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = $"ไม่พบผู้ใช้ ID: {id}" });
        }

        return Ok(new { data = user });
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "อีเมลนี้มีอยู่ในระบบแล้ว" });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Age = request.Age
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
            new { message = "สร้างผู้ใช้สำเร็จ", data = user });
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"ไม่พบผู้ใช้ ID: {id}" });
        }

        // Check email uniqueness (exclude current user)
        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
        {
            return BadRequest(new { message = "อีเมลนี้มีอยู่ในระบบแล้ว" });
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Age = request.Age;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "อัปเดตผู้ใช้สำเร็จ", data = user });
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"ไม่พบผู้ใช้ ID: {id}" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "ลบผู้ใช้สำเร็จ", deletedUser = user.Name });
    }
}

// Request DTOs
public class CreateUserRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Range(1, 150)]
    public int Age { get; set; }
}

public class UpdateUserRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Range(1, 150)]
    public int Age { get; set; }
}