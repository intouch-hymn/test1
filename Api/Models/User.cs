using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class User
{
    // Primary key - Entity Framework will auto-increment this
    public int Id { get; set; }

    // User's display name - required, max 100 characters
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    // User's email - required, must be valid email format, used for login
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // NEW FIELD: Stores hashed password (never store plain text passwords!)
    // This will be created when we generate a migration
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // User's age - must be between 1 and 150
    [Range(1, 150)]
    public int Age { get; set; }

    // user role
    public string Role { get; set; } = "User";

    // Automatically set when user is created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Set when user data is updated (nullable because it's not set on creation)
    public DateTime? UpdatedAt { get; set; }
}
    
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}