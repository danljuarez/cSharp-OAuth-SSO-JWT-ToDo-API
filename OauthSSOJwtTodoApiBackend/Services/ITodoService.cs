using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;

namespace OauthSSOJwtTodoApiBackend.Services
{
    public interface ITodoService
    {
        Task<(TodoDto? Todo, TodoOperationResult Result)> CreateTodoAsync(Guid userId, CreateTodoDto dto);
        Task<TodoOperationResult> DeleteTodoAsync(Guid userId, Guid todoId);
        Task<(TodoDto? Todo, TodoOperationResult Result)> GetTodoByIdAsync(Guid userId, Guid todoId);
        Task<IEnumerable<TodoDto>> GetUserTodosAsync(Guid userId);
        Task<(TodoDto? Todo, TodoOperationResult Result)> UpdateTodoAsync(Guid userId, Guid todoId, UpdateTodoDto dto);
    }
}