# AI Technical Interview Simulator

A beginner-friendly MVP for practising technical interview answers. Select a topic, complete five AI-generated questions, and receive answer feedback plus a final report.

## Technologies

- ASP.NET Core 8 Web API, Entity Framework Core, SQL Server
- React and Vite
- Google Gemini API

## Setup

1. Create a SQL Server database by running `database/schema.sql`, or create the database named in the API connection string.
2. In `backend/AIInterview.API/appsettings.json`, set `ConnectionStrings:DefaultConnection` and `Gemini:ApiKey`. You may also change `Gemini:Model`.
3. Install the frontend dependencies with `npm install` in `frontend/ai-interview-ui`.

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

`Gemini:ApiKey` is required for live AI responses. Do not commit a real key; prefer an environment variable such as `Gemini__ApiKey`. `ConnectionStrings:DefaultConnection` must point to an available SQL Server or LocalDB instance.
