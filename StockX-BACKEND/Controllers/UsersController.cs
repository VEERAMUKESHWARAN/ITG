using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using StockX.Model;
using StockX.Data;
using System.ComponentModel.DataAnnotations;

namespace StockX.Controllers
{
    public class UsersController : Controller
    {



        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }


        public string HashPassword(string plainPassword)
        {
            // Generate a hashed password
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        // Compare the plain-text password with the hashed password
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                return Conflict(new { message = "Email already exists" });

            // Hash the password before saving
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Save the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // Return success
            return Ok(new { message = "Login successful", user });
        }

        public class LoginRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }


        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserByUserId(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return NotFound();

            return user;
        }






        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            // Retrieve the user by UserID (not the primary key)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Remove the user from the database
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Verify the current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Unauthorized(new { message = "Current password is incorrect" });

            // Hash the new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Only update fields that are meant to be updated (excluding UserID)
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }


        public class ChangePasswordRequest
        {
            public string Email { get; set; }
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }



    }
}
