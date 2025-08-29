namespace ArticleApi.Dtos;

public class RegistrationDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Lga { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}