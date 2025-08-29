using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetPostMedia(int postId)
        {
            var media = await _mediaService.GetMediaByPostIdAsync(postId);
            return Ok(media);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia([FromForm] IFormFile file)
        {
            if (!await _mediaService.IsValidMediaFile(file))
            {
                return BadRequest("Invalid file type or size");
            }

            try
            {
                var filePath = await _mediaService.SaveMediaAsync(file);
                return Ok(new { Path = filePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpDelete("{mediaId}")]
        public async Task<IActionResult> DeleteMedia(int mediaId, int postId)
        {
            try
            {
                var media = (await _mediaService.GetMediaByPostIdAsync(postId))
                    .FirstOrDefault(m => m.Id == mediaId);

                if (media == null)
                    return NotFound();

                // Xóa file vật lý
                await _mediaService.DeleteMediaFileAsync(media.Path);

                // Xóa record trong database
                await _mediaService.DeleteMediaByIdAsync(postId, mediaId);

                return Ok("Media deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting media: {ex.Message}");
            }
        }
    }
}
