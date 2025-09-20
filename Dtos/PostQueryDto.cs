namespace ArticleApi.Dtos;

using ArticleApi.Models;

public class PostQueryDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public Visibility Visibility { get; set; } = Visibility.Public;
    public Status Status { get; set; } = Status.New;
    public Category Category { get; set; } = Category.General;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public List<string> ImageUrls { get; set; } = new();
}