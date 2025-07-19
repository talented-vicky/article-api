namespace ArticleApi.Controllers;

using CloudinaryDotNet; // has "Cloudinary" namespace
using CloudinaryDotNet.Actions; // for "ImageUploadParams" and "FileDescription" namespaces
using ArticleApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


[ApiController]
[Route("/api/upload")]
public class UploadController : ControllerBase
{
    private readonly Cloudinary _cloudinary;

    public UploadController(Cloudinary cloudinary) => _cloudinary = cloudinary;


    [HttpPost("media")]
    public async Task<IActionResult> UploadMedia([FromForm] IFormFile file)
    // "IFormFile" has methods; Length(file size), FileName(name), and OpenReadStream(reading the file uploaded)
    {
        if(file == null || file.Length == 0)
            return ApiResponse.NotFound("No File Uploaded");

        var allowedFileTypes = new[] 
        {
            "image/jpeg", "image/jpg", "image/png", "image/avif",
            "video/mp4", "video/mpeg", "video/quicktime"
        };

        if(!allowedFileTypes.Contains(file.ContentType.ToLower()))
            return ApiResponse.Error("Invalid File Type");

        await using var stream = file.OpenReadStream();

        var isVideo = file.ContentType.StartsWith("video");
        if(isVideo)
        {
            const long maxVideoLength = 10 * 1024 * 1024;
            if(file.Length > maxVideoLength)
                return ApiResponse.Error("Video Cannot Exceed 10MB");

            var videoUploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "posteet/videos"
            };

            var uploadedVideo = await _cloudinary.UploadAsync(videoUploadParams);
            if(uploadedVideo.StatusCode != System.Net.HttpStatusCode.OK)
                return ApiResponse.Error("Error Uploading Video");

            return ApiResponse.Success(
                new {Url = uploadedVideo.SecureUrl, Id = uploadedVideo.PublicId},
                "Video Uploaded"
            );
        }
        
        const long maxImageLength = 3 * 1024 * 1024;
        if(file.Length > maxImageLength)
            return ApiResponse.Error("Image Cannot Exceed 3MB");

        var imageUploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "posteet/images"
        };

        var uploadedFile = await _cloudinary.UploadAsync(imageUploadParams);
        if(uploadedFile.StatusCode != System.Net.HttpStatusCode.OK)
            return ApiResponse.Error("Image Upload Failed");
            
        return ApiResponse.Success( 
            new {Url = uploadedFile.SecureUrl.ToString(), Id = uploadedFile.PublicId},
            "Image Upload Successful" 
        );
    }
}