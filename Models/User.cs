namespace ArticleApi.Models;

public class User
{
    public int Id { get; set; }
    
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Lga { get; set; } = string.Empty;
    
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set;} = new List<Comment>();
}