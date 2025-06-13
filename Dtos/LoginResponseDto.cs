namespace ArticleApi.Dtos;

public class LoginResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}