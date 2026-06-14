namespace DevVault.Application.DTOs.Users;

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class UserResult
{
    public bool Success { get; set; }
    public UserResponseDto? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
