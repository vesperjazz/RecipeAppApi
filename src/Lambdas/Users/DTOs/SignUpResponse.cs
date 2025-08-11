using System;

namespace Lambdas.Users.DTOs;

public class SignUpResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
