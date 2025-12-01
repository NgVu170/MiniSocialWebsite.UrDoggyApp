using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Data;

namespace UrDoggy.Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tag/suggest?prefix=do
        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest([FromQuery] string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 1)
                return Ok(new List<object>());

            prefix = prefix.ToLower().Trim();

            var suggestions = await _context.Tags
                .Where(t => t.Name.StartsWith(prefix))
                .OrderBy(t => t.Name)
                .Take(8) // Giới hạn 8 gợi ý
                .Select(t => new
                {
                    name = t.Name,
                    display = "#" + t.Name,
                    count = _context.PostTags.Count(pt => pt.TagId == t.Id) // Số bài viết dùng tag này
                })
                .ToListAsync();

            return Ok(suggestions);
        }
    }
}