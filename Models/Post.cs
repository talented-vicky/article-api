namespace ArticleApi.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}