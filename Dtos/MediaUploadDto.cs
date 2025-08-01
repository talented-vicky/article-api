using System.ComponentModel.DataAnnotations;

namespace ArticleApi.Dtos;

public class MediaUploadDto
{
    [Required]
    public IFormFile File { get; set; }
}