using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace videoGameCharacterApi.Controllers;

/// <summary>
/// Upload a character picture. The file is saved in wwwroot/uploads and the API returns its public URL,
/// which the frontend then stores in Character.ImageUrl.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UploadsController(IWebHostEnvironment env) : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return Problem(detail: "Aucun fichier reçu.", statusCode: StatusCodes.Status400BadRequest);

        if (file.Length > MaxFileSize)
            return Problem(detail: "L'image doit faire moins de 5 Mo.", statusCode: StatusCodes.Status400BadRequest);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return Problem(detail: "Formats acceptés : jpg, png, gif, webp.", statusCode: StatusCodes.Status400BadRequest);

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using (var stream = System.IO.File.Create(Path.Combine(folder, fileName)))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new UploadResponse(url));
    }
}

public record UploadResponse(string Url);
