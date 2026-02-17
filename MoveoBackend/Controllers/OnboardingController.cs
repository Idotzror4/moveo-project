using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoveoBackend.Data;
using MoveoBackend.DTOs;
using MoveoBackend.Models;
using System.Security.Claims;

namespace MoveoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OnboardingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> SavePreferences(OnboardingDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var existingPreferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingPreferences != null)
            {
                existingPreferences.InterestedAssets = dto.InterestedAssets;
                existingPreferences.InvestorType = dto.InvestorType;
                existingPreferences.ContentTypes = dto.ContentTypes;
            }
            else
            {
                var preferences = new UserPreferences
                {
                    UserId = userId,
                    InterestedAssets = dto.InterestedAssets,
                    InvestorType = dto.InvestorType,
                    ContentTypes = dto.ContentTypes,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserPreferences.Add(preferences);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Preferences saved successfully" });
        }

        [HttpGet]
        public async Task<ActionResult<OnboardingDto>> GetPreferences()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preferences == null)
            {
                return NotFound(new { message = "No preferences found" });
            }

            return Ok(new OnboardingDto
            {
                InterestedAssets = preferences.InterestedAssets,
                InvestorType = preferences.InvestorType,
                ContentTypes = preferences.ContentTypes
            });
        }
    }
}