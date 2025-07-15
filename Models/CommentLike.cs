namespace ArticleApi.Models;

public class CommentLike
{
    public int Id { get; set; }
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    public int CommentId { get; set; }
    public Comment? Comment { get; set; }
}