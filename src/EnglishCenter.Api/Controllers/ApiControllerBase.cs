using EnglishCenter.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected PagedQuery GetPagedQuery() => new()
    {
        Page = int.TryParse(Request.Query["page"], out var p) ? p : 1,
        PageSize = int.TryParse(Request.Query["pageSize"], out var ps) ? ps : 20,
        SortBy = Request.Query["sortBy"].FirstOrDefault() ?? "updatedAt",
        SortDir = Request.Query["sortDir"].FirstOrDefault() ?? "desc",
    };
}
