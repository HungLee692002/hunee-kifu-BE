using EnglishCenter.Application.Abstractions;
using EnglishCenter.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers;

[ApiController]
[Route("api/v1/tuition-payments")]
[Authorize(Roles = "Admin,Accountant")]
public class TuitionPaymentsController : ApiControllerBase
{
    private readonly ITuitionService _service;

    public TuitionPaymentsController(ITuitionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult> GetList(CancellationToken ct) =>
        Ok(await _service.GetPaymentsPagedAsync(GetPagedQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TuitionPaymentDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _service.GetPaymentByIdAsync(id, ct));
}
