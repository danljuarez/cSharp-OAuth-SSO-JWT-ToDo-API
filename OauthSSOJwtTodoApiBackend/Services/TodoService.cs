using Microsoft.EntityFrameworkCore;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;
using OauthSSOJwtTodoApiBackend.Models.Entities;

namespace OauthSSOJwtTodoApiBackend.Services;

public class TodoService : ITodoService
{
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TodoDto>> GetUserTodosAsync(Guid userId)
    {
        return await _db.Todos
            .Where(t => t.UserId == userId)
            .Select(t => new TodoDto(t))
            .ToListAsync();
    }

    public async Task<(TodoDto? Todo, TodoOperationResult Result)> GetTodoByIdAsync(Guid userId, Guid todoId)
    {
        var todo = await _db.Todos
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == todoId);

        return todo == null
        ? (null, TodoOperationResult.NotFound)
        : (new TodoDto(todo), TodoOperationResult.Success);
    }

    public async Task<(TodoDto? Todo, TodoOperationResult Result)> CreateTodoAsync(Guid userId, CreateTodoDto dto)
    {
        var todo = new Todo
        {
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            IsCompleted = dto.IsCompleted,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();

        return (new TodoDto(todo), TodoOperationResult.Success);
    }

    public async Task<(TodoDto? Todo, TodoOperationResult Result)> UpdateTodoAsync(Guid userId, Guid todoId, UpdateTodoDto dto)
    {
        var todo = await _db.Todos.FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);
        if (todo is null)
            return (null, TodoOperationResult.NotFound);

        todo.Title = dto.Title ?? todo.Title;
        todo.Description = dto.Description ?? todo.Description;
        todo.IsCompleted = dto.IsCompleted ?? todo.IsCompleted;
        todo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (new TodoDto(todo), TodoOperationResult.Success);
    }

    public async Task<TodoOperationResult> DeleteTodoAsync(Guid userId, Guid todoId)
    {
        var todo = await _db.Todos.FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);
        if (todo == null)
            return TodoOperationResult.NotFound;

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();

        return TodoOperationResult.Success;
    }
}
