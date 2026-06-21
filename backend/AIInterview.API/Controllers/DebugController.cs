using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.API.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController(IGeminiService geminiService) : ControllerBase
{
    // Development diagnostic endpoint: never expose this endpoint publicly without access controls.
    [HttpPost("evaluate-answer")]
    public async Task<ActionResult<DebugEvaluateResponse>> Evaluate(DebugEvaluateRequest request) =>
        Ok(await geminiService.DebugEvaluateAnswerAsync(request.Topic, request.Concept, request.Difficulty, request.Question, request.Answer));
}
