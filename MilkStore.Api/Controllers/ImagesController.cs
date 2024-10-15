using Microsoft.AspNetCore.Mvc;
using MilkStore.Api.Services;

namespace MilkStore.Api.Controllers;

[Route("api/images")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly CloudinaryService _cloudinaryService;

    public ImagesController(CloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No image was uploaded.");
        }

        var imageUrl = await _cloudinaryService.UploadImageAsync(image);
        
        if (imageUrl == null)
        {
            return StatusCode(500, "Error uploading the image.");
        }

        return Ok(imageUrl);
    }
}