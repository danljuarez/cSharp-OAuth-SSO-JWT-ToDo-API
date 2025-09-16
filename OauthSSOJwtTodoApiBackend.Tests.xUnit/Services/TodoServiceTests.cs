using Microsoft.EntityFrameworkCore;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using OauthSSOJwtTodoApiBackend.Services;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Services;

public class TodoServiceTests
{
    private TodoDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TodoDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task GetUserTodosAsync_Should_ReturnTodos_When_TodosExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = GetInMemoryDbContext();
        context.Todos.AddRange(
            new Todo { Id = Guid.NewGuid(), Title = "Test 1", UserId = userId },
            new Todo { Id = Guid.NewGuid(), Title = "Test 2", UserId = userId }
        );
        await context.SaveChangesAsync();

        var service = new TodoService(context);

        // Act
        var result = await service.GetUserTodosAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserTodosAsync_Should_Return_EmptyList_When_NoTodosExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = GetInMemoryDbContext(); // get a clean DB with no todos
        var service = new TodoService(context);

        // Act
        var result = await service.GetUserTodosAsync(userId);

        // Assert
        Assert.NotNull(result);             // should not return null
        Assert.Empty(result);               // should be an empty list
    }

    [Fact]
    public async Task GetTodoByIdAsync_Should_ReturnTodo_When_TodoExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todoId = Guid.NewGuid();
        var context = GetInMemoryDbContext();
        context.Todos.Add(new Todo { Id = todoId, Title = "Todo", UserId = userId });
        await context.SaveChangesAsync();

        var service = new TodoService(context);

        // Act
        var (todo, result) = await service.GetTodoByIdAsync(userId, todoId);

        // Assert
        Assert.Equal(TodoOperationResult.Success, result);
        Assert.NotNull(todo);
        Assert.Equal(todoId, todo.Id);
    }

    [Fact]
    public async Task GetTodoByIdAsync_Should_ReturnNotFound_When_TodoDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new TodoService(context);

        // Act
        var (todo, result) = await service.GetTodoByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(todo);
        Assert.Equal(TodoOperationResult.NotFound, result);
    }

    [Fact]
    public async Task CreateTodoAsync_Should_ReturnTodo_When_ValidDataProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = GetInMemoryDbContext();
        var service = new TodoService(context);

        var dto = new CreateTodoDto
        {
            Title = "New Test Task",
            Description = "New Test Description",
            IsCompleted = false
        };

        // Act
        var (todo, result) = await service.CreateTodoAsync(userId, dto);

        // Assert
        Assert.Equal(TodoOperationResult.Success, result);
        Assert.NotNull(todo);
        Assert.Equal(dto.Title, todo.Title);
    }

    [Fact]
    public async Task CreateTodoAsync_Should_SetEmptyDescription_When_DescriptionIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var context = GetInMemoryDbContext();
        var service = new TodoService(context);

        var dto = new CreateTodoDto
        {
            Title = "No Test Task Description",
            Description = null,
            IsCompleted = false
        };

        // Act
        var (todo, result) = await service.CreateTodoAsync(userId, dto);

        // Assert
        Assert.Equal(TodoOperationResult.Success, result);
        Assert.NotNull(todo);
        Assert.Equal(string.Empty, todo.Description);
    }

    [Fact]
    public async Task CreateTodoAsync_Should_Throw_When_DtoIsNull()
    {
        // Arrange
        var service = new TodoService(GetInMemoryDbContext());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateTodoAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateTodoAsync_Should_UpdateTodo_When_TodoExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todoId = Guid.NewGuid();

        var context = GetInMemoryDbContext();
        context.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Old Title",
            Description = "Old Desc",
            UserId = userId,
            IsCompleted = false
        });
        await context.SaveChangesAsync();

        var service = new TodoService(context);

        var dto = new UpdateTodoDto
        {
            Title = "Updated Test Title",
            Description = "Updated Test Desc",
            IsCompleted = true
        };

        // Act
        var (todo, result) = await service.UpdateTodoAsync(userId, todoId, dto);

        // Assert
        Assert.Equal(TodoOperationResult.Success, result);
        Assert.NotNull(todo);
        Assert.Equal("Updated Test Title", todo.Title);
        Assert.True(todo.IsCompleted);
    }

    [Fact]
    public async Task UpdateTodoAsync_Should_ReturnNotFound_When_TodoDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new TodoService(context);

        var dto = new UpdateTodoDto
        {
            Title = "Nothing"
        };

        // Act
        var (todo, result) = await service.UpdateTodoAsync(Guid.NewGuid(), Guid.NewGuid(), dto);

        // Assert
        Assert.Equal(TodoOperationResult.NotFound, result);
        Assert.Null(todo);
    }

    [Fact]
    public async Task UpdateTodoAsync_Should_Throw_When_DtoIsNull()
    {
        // Arrange
        var service = new TodoService(GetInMemoryDbContext());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdateTodoAsync(Guid.NewGuid(), Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateTodoAsync_Should_KeepOriginalValues_When_FieldsAreNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todoId = Guid.NewGuid();
        var context = GetInMemoryDbContext();

        context.Todos.Add(new Todo
        {
            Id = todoId,
            UserId = userId,
            Title = "Original Test Title",
            Description = "Original Test Description",
            IsCompleted = false
        });
        await context.SaveChangesAsync();

        var service = new TodoService(context);

        var dto = new UpdateTodoDto
        {
            Title = null,
            Description = null,
            IsCompleted = null
        };

        // Act
        var (todo, result) = await service.UpdateTodoAsync(userId, todoId, dto);

        // Assert
        Assert.Equal(TodoOperationResult.Success, result);
        Assert.NotNull(todo);
        Assert.Equal("Original Test Title", todo.Title);
        Assert.Equal("Original Test Description", todo.Description);
        Assert.False(todo.IsCompleted);
    }

    [Fact]
    public async Task DeleteTodoAsync_Should_DeleteTodo_When_TodoExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todoId = Guid.NewGuid();

        var context = GetInMemoryDbContext();
        context.Todos.Add(new Todo
        {
            Id = todoId,
            Title = "Task To Delete",
            UserId = userId
        });
        await context.SaveChangesAsync();

        var service = new TodoService(context);

        // Act
        var result = await service.DeleteTodoAsync(userId, todoId);

        // Assert
        Assert.Equal(TodoOperationResult.Success, result);
        Assert.Empty(context.Todos.Where(t => t.Id == todoId));
    }

    [Fact]
    public async Task DeleteTodoAsync_Should_ReturnNotFound_When_TodoDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new TodoService(context);

        // Act
        var result = await service.DeleteTodoAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(TodoOperationResult.NotFound, result);
    }
}
