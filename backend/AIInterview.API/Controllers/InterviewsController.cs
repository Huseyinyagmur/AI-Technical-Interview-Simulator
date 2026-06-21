using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AIInterview.API.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterview.API.Controllers;

[ApiController]
[Route("api/interviews")]
[Authorize]
public class InterviewsController(IInterviewService interviewService, AppDbContext db, IPdfReportService pdfReportService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<StartInterviewResponse>> Start(StartInterviewRequest request)
    {
        try { return Ok(await interviewService.StartAsync(CurrentUserId(), request)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{sessionId:guid}/answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer(Guid sessionId, SubmitAnswerRequest request)
    {
        var result = await interviewService.SubmitAnswerAsync(CurrentUserId(), sessionId, request);
        return result is null ? NotFound(new { message = "Session or unanswered question was not found." }) : Ok(result);
    }

    [HttpGet("{sessionId:guid}/report")]
    public async Task<ActionResult<InterviewReportDto>> Report(Guid sessionId)
    {
        var report = await interviewService.GetReportAsync(CurrentUserId(), sessionId);
        return report is null ? NotFound(new { message = "The session does not exist or is not complete." }) : Ok(report);
    }

    [HttpGet("{sessionId:guid}/report/pdf")]
    public async Task<IActionResult> GetReportPdf(Guid sessionId)
    {
        var userId = CurrentUserId();
        var session = await db.InterviewSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session is null) return NotFound(new { message = "Mülakat bulunamadı." });
        if (session.UserId != userId) return Forbid();
        if (!session.IsCompleted) return BadRequest(new { message = "PDF raporu için mülakat tamamlanmalıdır." });
        var report = await interviewService.GetReportAsync(userId, sessionId);
        if (report is null) return NotFound();
        try { return File(pdfReportService.Generate(report, session.User?.FullName ?? "Kullanıcı"), "application/pdf", $"interview-report-{sessionId}.pdf"); }
        catch { return StatusCode(500, new { message = "PDF raporu oluşturulamadı." }); }
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<InterviewHistoryItemDto>>> History() => Ok(await interviewService.GetHistoryAsync(CurrentUserId()));
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
