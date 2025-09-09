namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;

/// <summary>
/// Request model to create a new Todo item.
/// </summary>
public class CreateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
