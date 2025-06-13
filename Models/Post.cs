namespace ArticleApi.Models;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}