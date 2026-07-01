using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NullReferenceException());

    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException();

    protected IActionResult Ok<T>(T data, string message = "") =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult OkResponse(string message) =>
            base.Ok(new ApiResponse { Success = true, Message = message });

    protected IActionResult Created<T>(string routeName, object routeValues, T data) =>
        base.CreatedAtRoute(routeName, routeValues, new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult BadRequest(string message) =>
        base.BadRequest(new ApiResponse { Message = message, Success = false });

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(new ApiResponse { Message = message, Success = false });

    protected IActionResult OkPaginated<T>(IEnumerable<T> items, int currentPage, int totalPages, int totalCount, string message = "") =>
            base.Ok(new PaginatedResponse<T>
            {
                Data = items,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Success = true,
                Message = message
            });
}
