# AI Technical Interview Simulator

A beginner-friendly MVP for practising technical interview answers. Select a topic, complete five AI-generated questions, and receive answer feedback plus a final report.

## Features

- Gemini-powered Turkish technical questions and structured answer evaluations
- Concept-aware interviews: OOP sessions rotate through encapsulation, inheritance, polymorphism, abstraction, interfaces, abstract classes, SOLID and design patterns
- Adaptive Junior → Mid → Senior difficulty based on running score average
- Recruiter-style report with strong topics, weak topics, recommended study areas and reached difficulty
- Persistent interview history and a dashboard with score history and topic performance charts
- SQL Server persistence through Entity Framework Core

## Architecture

React calls the ASP.NET Core Web API. `InterviewsController` delegates interview flow to `InterviewService`; `GeminiService` owns provider prompts, response logging and JSON evaluation parsing. EF Core maps `InterviewSession → InterviewQuestion → InterviewAnswer → AnswerEvaluation` to SQL Server. Each question records its concept and effective difficulty, which makes reports and analytics reproducible.

## Technologies

- ASP.NET Core 8 Web API, Entity Framework Core, SQL Server
- React and Vite
- Google Gemini API

## Setup

1. Create a SQL Server database by running `database/schema.sql`, or create the database named in the API connection string.
2. Copy `.env.example` to `.env` at the repository root. Set `GEMINI_API_KEY` to your Gemini API key; optionally set `GEMINI_MODEL` (for example, `gemini-2.5-flash`). `.env` is ignored by Git and must never be committed.
3. In `backend/AIInterview.API/appsettings.json`, set `ConnectionStrings:DefaultConnection`.
4. Install the frontend dependencies with `npm install` in `frontend/ai-interview-ui`.

## Run the backend

```powershell
cd backend/AIInterview.API
dotnet restore
dotnet run --urls http://localhost:5181
```

Open Swagger at `http://localhost:5181/swagger` in Development. The API has local fallbacks for questions and evaluations when the Gemini key is unset, which is useful for checking the flow.

## Run the frontend

```powershell
cd frontend/ai-interview-ui
npm run dev
```

The frontend expects the API at `http://localhost:5181/api`. Set `VITE_API_URL` if you use another URL.

## Configuration

The backend loads `GEMINI_API_KEY` and `GEMINI_MODEL` from a machine environment variable first, then from the repository `.env` file, and finally uses the safe `Gemini:ApiKey` / `Gemini:Model` fallback in `appsettings.json`. Do not put real keys in `appsettings.json`. `ConnectionStrings:DefaultConnection` must point to an available SQL Server or LocalDB instance.

## API

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/api/interviews/start` | Start a five-question interview |
| POST | `/api/interviews/{sessionId}/answer` | Evaluate an answer and receive the next adaptive question |
| GET | `/api/interviews/{sessionId}/report` | Get the full recruiter-style report |
| GET | `/api/interviews/history` | List completed interviews |
| GET | `/api/dashboard/summary` | Dashboard statistics and chart data |
| POST | `/api/debug/evaluate` | Development-only Gemini response diagnostic |

## Database schema

`InterviewSessions` stores interview lifecycle data; `InterviewQuestions` stores the selected concept and difficulty; answers and one-to-one evaluations are stored in `InterviewAnswers` and `AnswerEvaluations`. The initial SQL layout is in `database/schema.sql`; EF migrations are in `backend/AIInterview.API/Migrations`.

## Screenshots

Add product screenshots to `assets/` and reference them here when publishing the project.

## Future improvements

- Authenticated learner profiles and per-user dashboard filtering
- Automated tests for adaptive difficulty and Gemini response fixtures
- Exportable reports and richer accessible chart components
