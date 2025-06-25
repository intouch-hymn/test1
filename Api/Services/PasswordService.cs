using System.Security.Cryptography;
using System.Text;

namespace Api.Services
{
    // Interface for password hashing service
    public interface IPasswordService
    {
        string HashPassword(string password); // Takes a plain text password and returns a hashed version
        bool VerifyPassword(string password, string hashedPassword); // Verifies if a plain text password matches a stored hash
    }

    // Implementation of password hashing service
    public class PasswordService : IPasswordService // Implements the IPasswordService interface
    {
        // Takes a plain text password and returns a hashed version
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create()) //Sha256 is a cryptographic hash function
            {
                // Salt is a random value added to the password before hashing
                var salt = "YourAppSalt2025";
                
                // Combine password + salt before hashing
                var saltedPassword = password + salt; 
                
                // Convert to bytes and hash with SHA256
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                
                // Convert back to string for storage in database
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verifies if a plain text password matches a stored hash
        // Used during login to check if password is correct
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Hash the input password using the same method
            var hashOfInput = HashPassword(password);
            
            // Compare the hashes - if they match, password is correct
            return hashOfInput == hashedPassword;
        }
    }
}