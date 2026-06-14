namespace DevVault.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/*
    This class is used to encapsulate the result of a login attempt, 
    including success status, message, 
    and any relevant data (like the access token and its expiration time).
*/
public class LoginResult
{
    public bool Success { get; set; }
    public LoginResponseDto? Data { get; set; }
    public string? ErrorMessage { get; set; }
}
