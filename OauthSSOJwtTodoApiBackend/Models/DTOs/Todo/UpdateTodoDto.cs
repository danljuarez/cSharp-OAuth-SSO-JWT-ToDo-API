namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;

/// <summary>
/// Request model to update an existing Todo item.
/// </summary>
public class UpdateTodoDto
{
    public string? Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsCompleted { get; set; }
}
