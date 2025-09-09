namespace OauthSSOJwtTodoApiBackend.Models.Entities;

/// <summary>
/// Entity representing a to-do item created and managed by a user.
/// </summary>
public class Todo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
