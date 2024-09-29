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
    public class FiscalController : ControllerBase
    {
        private readonly AppDbContext _context; // Use AppDbContext here
        private readonly IConfiguration _configuration;

        public FiscalController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Fiscal
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Extract user ID from the token
            var userId = GetUserIdFromToken(HttpContext.Request.Headers["Authorization"].ToString());

            if (userId == null)
                return Unauthorized();

            var fiscals = await _context.Fiscaux
                .Where(f => f.Id_User == userId)
                .ToListAsync();

            return Ok(fiscals);
        }
        [HttpGet("getfiscal/{id}")]
        public async Task<IActionResult> GetFiscal(int id)
        {
            // Fetch the fiscal records for the given user ID
            var fiscals = await _context.Fiscaux
                .Where(f => f.Id_User == id)
                .ToListAsync();

            // Return the fiscals list, even if empty
            return Ok(fiscals);
        }
        // POST: api/Fiscal (Modified to delete existing records and then insert new ones)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<Fiscal> fiscals)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (fiscals == null || fiscals.Count == 0)
                return BadRequest("No fiscal data provided.");

            // Get the Id_User from the first fiscal object
            var idUser = fiscals.First().Id_User;

           

            // Delete all existing Fiscal records for the given Id_User
            var existingFiscals = await _context.Fiscaux.Where(f => f.Id_User == idUser).ToListAsync();
            _context.Fiscaux.RemoveRange(existingFiscals);

            // Add the new Fiscal records to the database
            foreach (var fiscal in fiscals)
            {
                // Optional: Fetch the Utilisateur if needed
                fiscal.Utilisateur = await _context.Users.FindAsync(fiscal.Id_User);
                _context.Fiscaux.Add(fiscal);
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok(fiscals); // Return the list of created records
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
