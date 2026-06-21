using AIInterview.API.DTOs;
using AIInterview.API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.API.Controllers;

[ApiController]
[Route("api/tracks")]
public class TracksController : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<InterviewTrackDto>> Get() => Ok(InterviewTrackCatalog.List());
}
