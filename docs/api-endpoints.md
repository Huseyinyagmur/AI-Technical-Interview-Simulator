# API endpoints

## `POST /api/interviews/start`

Starts a five-question interview. Body: `{ "topic": "C#", "difficulty": "Junior" }`. Topics are `C#`, `OOP`, `SQL`, and `ASP.NET Core Web API`.

## `POST /api/interviews/{sessionId}/answer`

Submits the answer for the displayed question. Body: `{ "questionId": 1, "answer": "..." }`. Returns the evaluation and, until question five, the next question.

## `GET /api/interviews/{sessionId}/report`

Returns the completed interview report, overall score, and per-question feedback. It returns 404 while the interview is incomplete.
