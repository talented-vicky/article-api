namespace ArticleApi.Models;

public class Comment
{
    public int Id { get; set; }
    public int Likes { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CommentedAt { get; set; } = DateTime.UtcNow;

    public int PostId { get; set; }
    public Post? Post { get; set;}

    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}