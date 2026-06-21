using AIInterview.API.Data;
using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIInterview.API.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController(IGeminiService geminiService, AppDbContext db) : ControllerBase
{
    // Development diagnostic endpoint: never expose this endpoint publicly without access controls.
    [HttpPost("evaluate-answer")]
    public async Task<ActionResult<DebugEvaluateResponse>> Evaluate(DebugEvaluateRequest request) =>
        Ok(await geminiService.DebugEvaluateAnswerAsync(request.Topic, request.Concept, request.Difficulty, request.Question, request.Answer));

    [HttpGet("last-evaluation")]
    public async Task<ActionResult<LastEvaluationDebugDto>> LastEvaluation()
    {
        var evaluation = await db.AnswerEvaluations.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        if (evaluation is null) return NotFound(new { message = "Henüz kaydedilmiş değerlendirme yok." });
        var result = new EvaluationDto(evaluation.Score, evaluation.Strengths, evaluation.Weaknesses, evaluation.ImprovementSuggestion, evaluation.Source, evaluation.ErrorMessage, evaluation.RawGeminiResponse);
        return Ok(new LastEvaluationDebugDto(evaluation.RawGeminiResponse, result, evaluation.Source, evaluation.ErrorMessage));
    }
}
