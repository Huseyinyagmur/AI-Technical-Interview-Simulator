using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.API.Controllers;

[ApiController]
[Route("api/interviews")]
public class InterviewsController(IInterviewService interviewService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<StartInterviewResponse>> Start(StartInterviewRequest request)
    {
        try { return Ok(await interviewService.StartAsync(request)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{sessionId:guid}/answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer(Guid sessionId, SubmitAnswerRequest request)
    {
        var result = await interviewService.SubmitAnswerAsync(sessionId, request);
        return result is null ? NotFound(new { message = "Session or unanswered question was not found." }) : Ok(result);
    }

    [HttpGet("{sessionId:guid}/report")]
    public async Task<ActionResult<InterviewReportDto>> Report(Guid sessionId)
    {
        var report = await interviewService.GetReportAsync(sessionId);
        return report is null ? NotFound(new { message = "The session does not exist or is not complete." }) : Ok(report);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<InterviewHistoryItemDto>>> History() => Ok(await interviewService.GetHistoryAsync());
}
