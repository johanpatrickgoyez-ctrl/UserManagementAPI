using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenString = _configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrWhiteSpace(tokenString))
                throw new InvalidOperationException("La configuración 'AppSettings:Token' no puede estar vacía.");

            byte[] keyBytes;

            // Intentar interpretar la clave como Base64. Si falla, usar UTF8.
            try
            {
                keyBytes = Convert.FromBase64String(tokenString!);
            }
            catch (FormatException)
            {
                keyBytes = Encoding.UTF8.GetBytes(tokenString!);
            }

            // Aceptar claves largas para HMAC-SHA512. Si la clave es más corta, derivar una clave de 64 bytes usando PBKDF2.
            byte[] signingKeyBytes;
            if (keyBytes.Length * 8 >= 512)
            {
                signingKeyBytes = keyBytes;
            }
            else
            {
                // Derivar una clave segura de 64 bytes (512 bits) desde la frase provista usando PBKDF2
                _logger?.LogWarning("La clave proporcionada es demasiado corta para HMAC-SHA512; se deriva una clave segura usando PBKDF2.");
                // Usamos una sal fija derivada del propio secreto para evitar requerir almacenamiento adicional.
                // Nota: idealmente la sal debe ser almacenada y rotada. Aquí se deriva para permitir frases legibles.
                var salt = SHA256.HashData(keyBytes).Take(16).ToArray();
                using var derive = new System.Security.Cryptography.Rfc2898DeriveBytes(keyBytes, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256);
                signingKeyBytes = derive.GetBytes(64); // 64 bytes = 512 bits
            }

            var key = new SymmetricSecurityKey(signingKeyBytes);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}