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
    public class VoteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> SubmitVote(VoteDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.SectionType == dto.SectionType);

            if (existingVote != null)
            {
                existingVote.IsPositive = dto.IsPositive;
                existingVote.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var vote = new Vote
                {
                    UserId = userId,
                    SectionType = dto.SectionType,
                    IsPositive = dto.IsPositive,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Votes.Add(vote);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Vote saved successfully" });
        }

        [HttpGet]
        public async Task<ActionResult> GetUserVotes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var votes = await _context.Votes
                .Where(v => v.UserId == userId)
                .Select(v => new
                {
                    sectionType = v.SectionType,
                    isPositive = v.IsPositive
                })
                .ToListAsync();

            return Ok(votes);
        }
    }
}