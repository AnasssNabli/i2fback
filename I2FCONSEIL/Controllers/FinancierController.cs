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

        // POST: api/Financier (Accepting a list of Financier objects)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<Financier> financiers)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var financier in financiers)
            {
                // Ensure the user exists in the database
                var userExists = await _context.Users.AnyAsync(u => u.Id == financier.Id_User);
                if (!userExists)
                    return BadRequest($"The specified user (ID: {financier.Id_User}) does not exist.");

                // Fetch the Utilisateur if needed (optional)
                financier.Utilisateur = await _context.Users.FindAsync(financier.Id_User);

                // Add each financier record to the database
                _context.Financiers.Add(financier);
            }

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
