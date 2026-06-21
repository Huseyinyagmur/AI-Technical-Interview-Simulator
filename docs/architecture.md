# Architecture

The React single-page app calls the ASP.NET Core Web API over HTTP. `InterviewsController` delegates business flow to `InterviewService`, which stores the interview aggregate through EF Core's `AppDbContext`. `GeminiService` is isolated behind `IGeminiService`, so AI calls and fallback behavior are kept out of the controller.

`InterviewSession` owns five `InterviewQuestion` records. Each question has one answer, and each answer has one evaluation. SQL Server stores this data through Entity Framework Core.

Prompt templates live in the repository-level `prompts` folder. If Gemini is not configured or responds unexpectedly, the service uses a local question/evaluation fallback so the MVP flow still completes.
