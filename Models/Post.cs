namespace ArticleApi.Models;

using NetTopologySuite.Geometries;


public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Views { get; set; }

    public Visibility Visibility { get; set; } = Visibility.Public;
    public Status Status { get; set; } = Status.New; 
    public Category Category { get; set; } = Category.General;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public Point? Location { get; set; } // gives (lat, long)

    public int UserId { get; set; }
    public User? User { get; set; }
    
    public List<PostImage> PostImages { get; set; } =  new();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}