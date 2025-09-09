namespace OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;

/// <summary>
/// Response model for a Todo item.
/// </summary>
public class TodoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid UserId { get; set; }

    // Optional: parameterless constructor (for serialization, etc.)
    public TodoDto() { }

    // Constructor to map from Todo entity
    public TodoDto(Entities.Todo todo)
    {
        Id = todo.Id;
        Title = todo.Title;
        Description = todo.Description;
        IsCompleted = todo.IsCompleted;
        CreatedAt = todo.CreatedAt;
        UpdatedAt = todo.UpdatedAt;
        UserId = todo.UserId;
    }
}
