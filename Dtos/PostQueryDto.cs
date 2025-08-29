namespace ArticleApi.Dtos;

public class PostQueryDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public string Visibility { get; set; } = "Public";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public List<string> ImageUrls { get; set; } = new();
}