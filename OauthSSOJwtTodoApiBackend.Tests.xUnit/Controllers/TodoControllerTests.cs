using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OauthSSOJwtTodoApiBackend.Controllers;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;
using OauthSSOJwtTodoApiBackend.Services;
using System.Security.Claims;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Controllers;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly TodoController _controller;

    public TodoControllerTests()
    {
        _mockTodoService = new Mock<ITodoService>();

        _controller = new TodoController(_mockTodoService.Object);

        var userId = Guid.NewGuid().ToString(); // Setup fake user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }
        , "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_Should_Return_OkResult_With_Todos_When_ValidUser()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId); // Test fails if claim is missing

        var todos = new List<TodoDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Test 1", Description = "Test Desc", IsCompleted = false },
            new() { Id = Guid.NewGuid(), Title = "Test 2", Description = "Test Desc", IsCompleted = true }
        };

        _mockTodoService
            .Setup(s => s.GetUserTodosAsync(userId.Value))
            .ReturnsAsync(todos);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTodos = Assert.IsAssignableFrom<IEnumerable<TodoDto>>(okResult.Value);
        Assert.Equal(2, returnedTodos.Count());
    }

    [Fact]
    public async Task GetAll_Should_Return_Unauthorized_When_UserIdMissing()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user is set
        };

        // Act
        var result = await _controller.GetAll();

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID claim missing or invalid.", unauthorized.Value);
    }

    [Fact]
    public async Task Get_Should_Return_OkResult_With_Todo_When_Found()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();

        var mockTodo = new TodoDto
        {
            Id = todoId,
            Title = "Test Task",
            Description = "Test Desc",
            IsCompleted = false
        };

        _mockTodoService
            .Setup(s => s.GetTodoByIdAsync(userId.Value, todoId))
            .ReturnsAsync((mockTodo, TodoOperationResult.Success));

        // Act
        var result = await _controller.Get(todoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<TodoDto>(okResult.Value);
        Assert.Equal(todoId, returned.Id);
    }

    [Fact]
    public async Task Get_Should_Return_NotFound_When_Todo_Does_Not_Exist()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var nonExistentTodoId = Guid.NewGuid();

        _mockTodoService
            .Setup(s => s.GetTodoByIdAsync(userId.Value, nonExistentTodoId))
            .ReturnsAsync((null, TodoOperationResult.NotFound));

        // Act
        var result = await _controller.Get(nonExistentTodoId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Get_Should_Return_InternalServerError_When_UnexpectedResult()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var unknownTodoId = Guid.NewGuid();

        _mockTodoService
            .Setup(s => s.GetTodoByIdAsync(userId.Value, unknownTodoId))
            .ReturnsAsync((null, (TodoOperationResult)999));

        // Act
        var result = await _controller.Get(unknownTodoId);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Get_Should_Return_Unauthorized_When_UserId_Claim_Missing()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user is set
        };

        // Act
        var result = await _controller.Get(Guid.NewGuid());

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID claim missing or invalid.", unauthorized.Value);
    }

    [Fact]
    public async Task Create_Should_Return_OkResult_With_CreatedTodo_When_Valid()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();
        var dto = new CreateTodoDto
        {
            Title = "New Test Task",
            Description = "From unit tests",
            IsCompleted = true
        };

        var createdTodo = new TodoDto
        {
            Id = todoId,
            Title = dto.Title,
            Description = dto.Description,
            IsCompleted = dto.IsCompleted,
        };

        _mockTodoService
            .Setup(s => s.CreateTodoAsync(userId.Value, dto))
            .ReturnsAsync((createdTodo, TodoOperationResult.Success));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<TodoDto>(okResult.Value);
        Assert.Equal(createdTodo.Id, returned.Id);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Input_Is_Invalid()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var dto = new CreateTodoDto
        {
            Title = "", // invalid title
            Description = "Test Desc",
            IsCompleted = false
        };

        _mockTodoService
            .Setup(s => s.CreateTodoAsync(userId.Value, dto))
            .ReturnsAsync((null, TodoOperationResult.InvalidInput));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_Should_Return_Unauthorized_When_UserId_Is_Missing()
    {
        // Arrange
        var dto = new CreateTodoDto
        {
            Title = "Testing unauthorized",
            Description = "",
            IsCompleted = false
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user is set
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID claim missing or invalid.", unauthorized.Value);
    }

    [Fact]
    public async Task Update_Should_Return_OkResult_With_UpdatedTodo_When_Valid()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();

        var dto = new UpdateTodoDto
        {
            Title = "Updated Title",
            Description = "Updated Desc",
            IsCompleted = true
        };

        var updatedTodo = new TodoDto
        {
            Id = todoId,
            Title = dto.Title!,
            Description = dto.Description!,
            IsCompleted = dto.IsCompleted!.Value
        };

        _mockTodoService
            .Setup(s => s.UpdateTodoAsync(userId.Value, todoId, dto))
            .ReturnsAsync((updatedTodo, TodoOperationResult.Success));

        // Act
        var result = await _controller.Update(todoId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<TodoDto>(okResult.Value);
        Assert.Equal(updatedTodo.Title, returned.Title);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Todo_NotExists()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();

        var dto = new UpdateTodoDto
        {
            Title = "Test Title",
            Description = null,
            IsCompleted = null
        };

        _mockTodoService
            .Setup(s => s.UpdateTodoAsync(userId.Value, todoId, dto))
            .ReturnsAsync((null, TodoOperationResult.NotFound));

        // Act
        var result = await _controller.Update(todoId, dto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_InvalidInput()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();

        var dto = new UpdateTodoDto
        {
            Title = "", // invalid title
            Description = "desc",
            IsCompleted = false
        };

        _mockTodoService
            .Setup(s => s.UpdateTodoAsync(userId.Value, todoId, dto))
            .ReturnsAsync((null, TodoOperationResult.InvalidInput));

        // Act
        var result = await _controller.Update(todoId, dto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_Unauthorized_When_UserId_Missing()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var dto = new UpdateTodoDto
        {
            Title = "Test Title",
            Description = null,
            IsCompleted = false
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user is set
        };

        // Act
        var result = await _controller.Update(todoId, dto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID claim missing or invalid.", unauthorized.Value);
    }

    [Fact]
    public async Task Delete_Should_Return_Ok_When_Success()
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();

        _mockTodoService
            .Setup(s => s.DeleteTodoAsync(userId.Value, todoId))
            .ReturnsAsync(TodoOperationResult.Success);

        // Act
        var result = await _controller.Delete(todoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Task deleted", okResult.Value);
    }

    [Theory]
    [InlineData(TodoOperationResult.NotFound, typeof(NotFoundResult))]
    [InlineData(TodoOperationResult.InvalidInput, typeof(BadRequestResult))]
    public async Task Delete_Should_Return_Correct_Status_For_KnownFailures(
        TodoOperationResult serviceResult,
        Type expectedActionResultType)
    {
        // Arrange
        var userId = _controller.User.TryGetUserId();
        Assert.NotNull(userId);

        var todoId = Guid.NewGuid();

        _mockTodoService
            .Setup(s => s.DeleteTodoAsync(userId.Value, todoId))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Delete(todoId);

        // Assert
        Assert.IsType(expectedActionResultType, result);
    }

    [Fact]
    public async Task Delete_Should_Return_Unauthorized_When_UserId_Is_Missing()
    {
        // Arrange
        var todoId = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user is set
        };

        // Act
        var result = await _controller.Delete(todoId);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID claim missing or invalid.", unauthorized.Value);
    }

    [Fact]
    public async Task Delete_Should_Return_InternalServerError_When_UnexpectedResult()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var fakeUserId = Guid.NewGuid();

        // Set fake User ID in HttpContext
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, fakeUserId.ToString())
    };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Setup unexpected return value
        _mockTodoService
            .Setup(s => s.DeleteTodoAsync(fakeUserId, todoId))
            .ReturnsAsync((TodoOperationResult)777);

        // Act
        var result = await _controller.Delete(todoId);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}
