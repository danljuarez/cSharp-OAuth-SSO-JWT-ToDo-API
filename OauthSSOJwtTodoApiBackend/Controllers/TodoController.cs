using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Todo;
using OauthSSOJwtTodoApiBackend.Services;

namespace OauthSSOJwtTodoApiBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Base-level auth
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService) => _todoService = todoService;

    /// <summary>
    /// Extracts the user ID from the authenticated principal (JWT or cookie).
    /// Returns null if the claim is missing or malformed.
    /// </summary>
    private Guid? UserId => User.TryGetUserId();

    // All roles can view their own todos
    [HttpGet]
    [Authorize(Roles = "User,Manager,Admin")]
    public async Task<IActionResult> GetAll()
    {
        if (UserId is not Guid userId || userId == Guid.Empty)
            return Unauthorized("User ID claim missing or invalid.");

        var todos = await _todoService.GetUserTodosAsync(userId);
        return Ok(todos);
    }

    // All roles can get a specific todo
    [HttpGet("{id}")]
    [Authorize(Roles = "User,Manager,Admin")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (UserId is not Guid userId || userId == Guid.Empty)
            return Unauthorized("User ID claim missing or invalid.");

        var (todo, result) = await _todoService.GetTodoByIdAsync(userId, id);
        return MapTodoResult(result, todo);
    }

    // All roles can create
    [HttpPost]
    [Authorize(Roles = "User,Manager,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
    {
        if (UserId is not Guid userId || userId == Guid.Empty)
            return Unauthorized("User ID claim missing or invalid.");

        var (created, result) = await _todoService.CreateTodoAsync(userId, dto);

        return MapTodoResult(result, created);
    }

    // Only Manager and Admin can update
    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoDto dto)
    {
        if (UserId is not Guid userId || userId == Guid.Empty)
            return Unauthorized("User ID claim missing or invalid.");

        var (updated, result) = await _todoService.UpdateTodoAsync(userId, id, dto);

        return MapTodoResult(result, updated);
    }

    // Only Admin can delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (UserId is not Guid userId || userId == Guid.Empty)
            return Unauthorized("User ID claim missing or invalid.");

        var result = await _todoService.DeleteTodoAsync(userId, id);

        return result switch
        {
            TodoOperationResult.Success => Ok("Task deleted"),
            TodoOperationResult.NotFound => NotFound(),
            TodoOperationResult.InvalidInput => BadRequest(),
            _ => StatusCode(500)
        };
    }

    // Helper to map service result to IActionResult
    private IActionResult MapTodoResult<T>(TodoOperationResult result, T? value)
    {
        return result switch
        {
            TodoOperationResult.Success => Ok(value),
            TodoOperationResult.NotFound => NotFound(),
            TodoOperationResult.InvalidInput => BadRequest(),
            _ => StatusCode(500)
        };
    }
}

