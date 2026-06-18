namespace DevVault.Application.DTOs.Common;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
