using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Document_Management_System.Data;
using Document_Management_System.DTOs;
using Document_Management_System.Models;
using Document_Management_System.Services;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterDTO request)
        {
            if (await _context.Users
                .AnyAsync(x => x.Email == request.Email))
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),
                Role = "Candidate"
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok("Registration successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == request.Email);

            if (user == null)
                return Unauthorized();

            bool validPassword =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!validPassword)
                return Unauthorized();

            var token =
                _jwtService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                User = user.Email,
                Role = user.Role
            });
        }
    }
}