using Microsoft.AspNetCore.Mvc;
using HomeBankingApi.Data;
using HomeBankingApi.models;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace HomeBankingApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            #region validators
            if (string.IsNullOrWhiteSpace(user.FirstName) || !Regex.IsMatch(user.FirstName, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
                return BadRequest(new { message = "El nombre no es válido." });

            if (string.IsNullOrWhiteSpace(user.LastName) || !Regex.IsMatch(user.LastName, "^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
                return BadRequest(new { message = "El apellido no es válido." });

            if (string.IsNullOrWhiteSpace(user.Email) || !Regex.IsMatch(user.Email, @"^[\w\.-]+@[\w\.-]+\.\w+$"))
                return BadRequest(new { message = "El correo electrónico no es válido." });

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { message = "El email ya está registrado." });

            if (string.IsNullOrWhiteSpace(user.PasswordHash) || user.PasswordHash.Length < 8)
                return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres." });

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { message = "El email ya está registrado." });

            #endregion

            user.CreatedDate = DateTime.UtcNow;
            user.LastUpdatedDate = DateTime.UtcNow;
            user.IsVerified = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateVerificationToken(user.Email);
            Console.WriteLine($"Generated Verification Token: {token}");

            SendVerificationEmail(user.Email, token);

            return Ok(new { message = "Usuario registrado exitosamente. Por favor, verifica tu correo electrónico." });
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            Console.WriteLine($"🔹 Token recibido: {token}");

            var principal = ValidateVerificationToken(token);
            if (principal == null)
                return BadRequest(new { message = "Token inválido o expirado." });

            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type} - Value: {claim.Value}");
            }

            var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"🔹 Email extraído del token: {email}");

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Error al extraer email del token." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado." });

            user.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tu cuenta ha sido verificada exitosamente." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement loginData)
        {
            if (!loginData.TryGetProperty("email", out JsonElement emailElement) || !loginData.TryGetProperty("passwordHash", out JsonElement passwordElement))
                return BadRequest(new { message = "El email y la contraseña son obligatorios." });

            string email = emailElement.GetString()?.Trim();
            string passwordHash = passwordElement.GetString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordHash) || !Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return BadRequest(new { message = "El email no es válido." });

            if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length < 8)
                return BadRequest(new { message = "La contraseña no es válida." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || passwordHash != user.PasswordHash)
                return Unauthorized(new { message = "Credenciales incorrectas." });

            if (!user.IsVerified)
                return Unauthorized(new { message = "Por favor, verifica tu email antes de iniciar sesión." });

            return Ok(new
            {
                message = "Inicio de sesión exitoso.",
                userId = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email
            });
        }

        private ClaimsPrincipal ValidateVerificationToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                return tokenHandler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        private string GenerateVerificationToken(string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine($"Token Generado: {generatedToken}");
            return generatedToken;
        }

        private void SendVerificationEmail(string email, string token)
        {
            var verificationLink = $"https://localhost:44418/api/auth/verify?token={token}";
            var subject = "Verifica tu email";
            var body = $"Haz clic en el siguiente enlace para verificar tu email: <a href='{verificationLink}'>Verificar Email</a>";

            var smtp = new SmtpClient
            {
                Host = _configuration["EmailSettings:SmtpServer"],
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:FromEmail"], _configuration["EmailSettings:Password"])
            };

            var message = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromEmail"], "Soporte AgroByte"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(email);
            smtp.Send(message);
        }
    }
}
