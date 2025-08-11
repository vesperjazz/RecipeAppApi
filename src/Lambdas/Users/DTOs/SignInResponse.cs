using System;

namespace Lambdas.Users.DTOs;

public class SignInResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public DateTime IssuedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
