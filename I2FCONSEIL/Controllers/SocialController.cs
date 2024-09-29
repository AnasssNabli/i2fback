using I2FCONSEIL.DATA;
using I2FCONSEIL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace I2FCONSEIL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure that the controller is secured and requires authentication
    public class SocialController : ControllerBase
    {
        private readonly AppDbContext _context; // Use AppDbContext here
        private readonly IConfiguration _configuration;

        public SocialController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Social
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Extract user ID from the token
            var userId = GetUserIdFromToken(HttpContext.Request.Headers["Authorization"].ToString());

            if (userId == null)
                return Unauthorized();

            var socials = await _context.Sociaux
                .Where(s => s.Id_User == userId)
                .ToListAsync();

            return Ok(socials);
        }
        // Modified to return an empty list if no records are found
        [HttpGet("getsocial/{id}")]
        public async Task<IActionResult> Getsocial(int id)
        {
            // Fetch the social records for the given user ID
            var socials = await _context.Sociaux
                .Where(s => s.Id_User == id)
                .ToListAsync();

            // Return the socials list, even if empty
            return Ok(socials);
        }

        // POST: api/Social (Modified to delete existing records and then insert new ones)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<Social> socials)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (socials == null || socials.Count == 0)
                return BadRequest("No social data provided.");

            // Get the Id_User from the first social object
            var idUser = socials.First().Id_User;

            // Delete all existing Social records for the given Id_User
            var existingSocials = await _context.Sociaux.Where(s => s.Id_User == idUser).ToListAsync();
            _context.Sociaux.RemoveRange(existingSocials);

            // Add the new Social records to the database
            foreach (var social in socials)
            {
                // Optional: Fetch the Utilisateur if needed
                social.Utilisateur = await _context.Users.FindAsync(social.Id_User);
                _context.Sociaux.Add(social);
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok(socials); // Return the list of created records
        }

        // Utility method to extract user ID from JWT token
        private int? GetUserIdFromToken(string authorization)
        {
            var token = authorization?.StartsWith("Bearer ") == true
                ? authorization.Substring("Bearer ".Length).Trim()
                : null;

            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"]);
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtConfig:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return null;
                }

                return int.Parse(userIdClaim.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
