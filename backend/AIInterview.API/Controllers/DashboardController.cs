using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IInterviewService interviewService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary() => Ok(await interviewService.GetDashboardAsync());
}
