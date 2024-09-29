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
    public class FinancierController : ControllerBase
    {
        private readonly AppDbContext _context; // Use AppDbContext here
        private readonly IConfiguration _configuration;

        public FinancierController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Financier
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Extract user ID from the token
            var userId = GetUserIdFromToken(HttpContext.Request.Headers["Authorization"].ToString());

            if (userId == null)
                return Unauthorized();

            var financiers = await _context.Financiers
                .Where(f => f.Id_User == userId)
                .ToListAsync();

            return Ok(financiers);
        }
        [HttpGet("getfinancier/{id}")]
        public async Task<IActionResult> GetFinancier(int id)
        {
            // Fetch the financier records for the given user ID
            var financiers = await _context.Financiers
                .Where(f => f.Id_User == id)
                .ToListAsync();

            // Return the financiers list, even if empty
            return Ok(financiers);
        }
        // POST: api/Financier (Modified to delete existing records and then insert new ones)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<Financier> financiers)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (financiers == null || financiers.Count == 0)
                return BadRequest("No financier data provided.");

            // Get the Id_User from the first financier object
            var idUser = financiers.First().Id_User;

            // Ensure the user exists in the database
            

            // Delete all existing Financier records for the given Id_User
            var existingFinanciers = await _context.Financiers.Where(f => f.Id_User == idUser).ToListAsync();
            _context.Financiers.RemoveRange(existingFinanciers);

            // Add the new Financier records to the database
            foreach (var financier in financiers)
            {
                // Optional: Fetch the Utilisateur if needed
                financier.Utilisateur = await _context.Users.FindAsync(financier.Id_User);
                _context.Financiers.Add(financier);
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok(financiers); // Return the list of created records
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
