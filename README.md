# AI Technical Interview Simulator

A beginner-friendly MVP for practising technical interview answers. Select a topic, complete five AI-generated questions, and receive answer feedback plus a final report.

## Features

- Gemini-powered Turkish technical questions and structured answer evaluations
- Concept-aware interviews: OOP sessions rotate through encapsulation, inheritance, polymorphism, abstraction, interfaces, abstract classes, SOLID and design patterns
- Adaptive Junior → Mid → Senior difficulty based on running score average
- Recruiter-style report with strong topics, weak topics, recommended study areas and reached difficulty
- Persistent interview history and a dashboard with score history and topic performance charts
- SQL Server persistence through Entity Framework Core
- Career-oriented interview tracks, adaptive levels, question-bank variation and study-roadmap reports

## Interview tracks

| Track | Focus |
| --- | --- |
| C# Backend Developer | C#, OOP, LINQ, EF Core, Web API and SQL |
| ASP.NET Core Developer | API design, middleware, DI, security and EF Core |
| SQL Developer | Querying, transactions, modelling and optimisation |
| Software Engineering Fundamentals | Git, testing, Clean Code, Agile and CI/CD |
| Computer Vision Engineer | Image processing, YOLO, OCR, CNN, metrics and deployment |
| Mixed Technical Interview | Balanced questions across all technical domains |

Each interview records concept/domain coverage, adaptive difficulty progression, topic breakdown and a recommended study roadmap.

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
4. Copy `frontend/ai-interview-ui/.env.example` to `frontend/ai-interview-ui/.env`. The default API URL is `http://localhost:5268/api`.
5. Install the frontend dependencies with `npm install` in `frontend/ai-interview-ui`.

## Run the backend

```powershell
cd backend/AIInterview.API
dotnet restore
dotnet ef database update
dotnet run
```

Open Swagger at `http://localhost:5181/swagger` in Development. The API has local fallbacks for questions and evaluations when the Gemini key is unset, which is useful for checking the flow.

## Run the frontend

```powershell
cd frontend/ai-interview-ui
npm run dev
```

The frontend expects the API at `http://localhost:5268/api`. Set `VITE_API_BASE_URL` if you use another URL.

Open `http://localhost:5173` to use the full React interview flow. The frontend uses `VITE_API_BASE_URL`, defaulting to `http://localhost:5268/api`.

## Configuration

The backend loads `GEMINI_API_KEY` and `GEMINI_MODEL` from a machine environment variable first, then from the repository `.env` file, and finally uses the safe `Gemini:ApiKey` / `Gemini:Model` fallback in `appsettings.json`. Do not put real keys in `appsettings.json`. `ConnectionStrings:DefaultConnection` must point to an available SQL Server or LocalDB instance.

## Common local errors

- **Invalid column name `CompletedAtUtc`, `Concept`, or `Difficulty`:** run `dotnet ef database update` from `backend/AIInterview.API`. See [database reset notes](docs/database-reset.md) if the local schema is damaged.
- **Gemini API key missing:** add `GEMINI_API_KEY` to the root `.env` file and restart the API.
- **CORS or backend connection error:** verify the API is running at `http://localhost:5268` and the frontend `.env` uses `VITE_API_BASE_URL=http://localhost:5268/api`.

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

## Architecture diagram

```text
React UI → ASP.NET Core API → InterviewService → GeminiService
                    ↓                 ↓
                SQL Server ← Entity Framework Core
```

## Future improvements

- Authenticated learner profiles and per-user dashboard filtering
- Automated tests for adaptive difficulty and Gemini response fixtures
- Exportable reports and richer accessible chart components
