# AI Technical Interview Simulator

Yapay zeka destekli teknik mülakat simülatörü. Kullanıcıların gerçek teknik mülakat deneyimi yaşayabilmesini, cevaplarını Gemini ile değerlendirebilmesini ve kişiselleştirilmiş gelişim raporları alabilmesini sağlayan full-stack web uygulaması.

## Öne Çıkan Özellikler

- Gemini-powered Turkish technical questions and structured answer evaluations
- Concept-aware interviews: OOP sessions rotate through encapsulation, inheritance, polymorphism, abstraction, interfaces, abstract classes, SOLID and design patterns
- Adaptive Junior → Mid → Senior difficulty based on running score average
- Recruiter-style report with strong topics, weak topics, recommended study areas and reached difficulty
- Persistent interview history and a dashboard with score history and topic performance charts
- SQL Server persistence through Entity Framework Core
- Career-oriented interview tracks, adaptive levels, question-bank variation and study-roadmap reports

## Proje Özeti

Bu proje, teknik mülakat pratiğini yalnızca soru-cevap akışı olmaktan çıkarıp ölçülebilir bir gelişim deneyimine dönüştürür. Kullanıcılar bir kariyer track'i seçer, seviyelerine uygun soruları yanıtlar, AI değerlendirmesi ve detaylı raporlarla eksiklerini görür.

### Temel Kabiliyetler

- Gemini ile teknik doğruluk, açıklama kalitesi, örnek kullanımı ve problem çözme yaklaşımının değerlendirilmesi
- Kullanıcının ortalama puanına göre Junior, Mid ve Senior seviyeleri arasında adaptif ilerleme
- JWT ile güvenli giriş/kayıt ve kullanıcı bazlı dashboard, history ve raporlar
- Tamamlanan mülakatlar için profesyonel A4 PDF rapor çıktısı
- Track, konu, konsept ve zorluk seviyesine göre kalıcı performans analitiği

## Desteklenen Mülakat Trackleri

| Track | Focus |
| --- | --- |
| C# Backend Developer | C#, OOP, LINQ, EF Core, Web API and SQL |
| ASP.NET Core Developer | API design, middleware, DI, security and EF Core |
| SQL Developer | Querying, transactions, modelling and optimisation |
| Software Engineering Fundamentals | Git, testing, Clean Code, Agile and CI/CD |
| Computer Vision Engineer | Image processing, YOLO, OCR, CNN, metrics and deployment |
| Mixed Technical Interview | Balanced questions across all technical domains |

Each interview records concept/domain coverage, adaptive difficulty progression, topic breakdown and a recommended study roadmap.

## Mimari

React calls the ASP.NET Core Web API. `InterviewsController` delegates interview flow to `InterviewService`; `GeminiService` owns provider prompts, response logging and JSON evaluation parsing. EF Core maps `InterviewSession → InterviewQuestion → InterviewAnswer → AnswerEvaluation` to SQL Server. Each question records its concept and effective difficulty, which makes reports and analytics reproducible.

## Teknolojiler

- ASP.NET Core 8 Web API, Entity Framework Core, SQL Server
- React and Vite
- Google Gemini API

## Kurulum

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

## Authentication

Register with `POST /api/auth/register` and login with `POST /api/auth/login`. Both return a JWT. The frontend stores it in localStorage and sends it as a Bearer token. Interview, history and dashboard endpoints require authentication and only return the current user's sessions.

Add these values to the root `.env` file (never commit it):

```env
JWT_SECRET=replace_with_a_long_random_secret
JWT_ISSUER=AIInterviewSimulator
JWT_AUDIENCE=AIInterviewSimulatorUsers
JWT_EXPIRES_MINUTES=120
```

## PDF report export

Completed interview reports can be downloaded from the report screen. The protected endpoint is `GET /api/interviews/{sessionId}/report/pdf`; it generates an A4 PDF containing summary scores, study recommendations and per-question feedback. The endpoint requires a valid JWT and verifies that the requested interview belongs to the current user.

If PDF download fails, verify that the interview is completed and that the browser session is still valid. The UI explains expired-session, missing-report, ownership and incomplete-interview errors separately; backend logs include the underlying PDF generation exception.

## Modern UI

The frontend uses a consistent SaaS-style design system across authentication, track selection, interview reports, dashboard and history. It includes responsive layouts, visible loading/error states, accessible focus styles, modern cards and protected user-specific flows.

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

## Kullanıcı Akışı

1. Kullanıcı kayıt olur veya giriş yapar.
2. Kariyer hedefiyle uyumlu bir mülakat track'i seçer.
3. Başlangıç seviyesini belirler; sistem sonraki sorularda zorluğu puana göre uyarlar.
4. Gemini, track ve konsept bağlamında teknik sorular üretir.
5. Tamamlanan görüşme; dashboard, geçmiş, ayrıntılı rapor ve PDF çıktısı olarak kullanıcıya sunulur.

## Authentication

Sistem JWT Bearer authentication kullanır. `register` ve `login` işlemleri token üretir; interview, history, dashboard, report ve PDF endpoint'leri sadece giriş yapan kullanıcının kendi verilerine erişmesine izin verir.

## Adaptif Zorluk

| Ortalama skor | Davranış |
| --- | --- |
| 90 ve üzeri | Zorluk bir seviye artar |
| 70 - 89 | Aynı seviyede devam eder |
| 70 altı | Zorluk bir seviye düşer |

## Raporlama ve PDF

Rapor ekranı ortalama skor, güçlü/gelişim alanları, önerilen çalışma planı, zorluk ilerlemesi ve soru bazlı analiz içerir. `PDF Raporu İndir` aksiyonu, yalnızca rapor sahibinin indirebildiği profesyonel A4 çıktıyı üretir.

## Dashboard

Dashboard; tamamlanan görüşme sayısı, ortalama/en yüksek/en düşük skor, puan geçmişi ve konu performanslarını kullanıcı bazında gösterir.

## API Endpointleri

| Grup | Endpoint | Açıklama |
| --- | --- | --- |
| Auth | `POST /api/auth/register` | Yeni kullanıcı kaydı |
| Auth | `POST /api/auth/login` | JWT token üretir |
| Interview | `POST /api/interviews/start` | Yeni görüşme başlatır |
| Interview | `POST /api/interviews/{id}/answer` | Cevabı değerlendirir |
| Report | `GET /api/interviews/{id}/report` | Ayrıntılı raporu döndürür |
| PDF | `GET /api/interviews/{id}/report/pdf` | Korumalı PDF raporu indirir |
| History | `GET /api/interviews/history` | Kullanıcının geçmişini döndürür |
| Dashboard | `GET /api/dashboard/summary` | Kullanıcı performans özetini döndürür |

## Ortam Değişkenleri

```env
GEMINI_API_KEY=your_gemini_api_key_here
GEMINI_MODEL=gemini-2.5-flash
JWT_SECRET=your_super_secret_key_here
JWT_ISSUER=AIInterviewSimulator
JWT_AUDIENCE=AIInterviewSimulatorUsers
JWT_EXPIRES_MINUTES=120
```

Gerçek anahtarları asla repoya eklemeyin; kök `.env` dosyası `.gitignore` ile korunur.

## Ekran Görüntüleri

### Giriş Ekranı

> `assets/screenshots/login.png` eklenecek.

### Dashboard

> `assets/screenshots/dashboard.png` eklenecek.

### Interview ve Report

> `assets/screenshots/interview-report.png` eklenecek.

## Gelecek Geliştirmeler

- Docker ve GitHub Actions CI/CD
- Sesli mülakat modu
- OpenAI sağlayıcı desteği
- CV analyzer
- Çoklu dil desteği

## Neden Bu Proje?

AI Technical Interview Simulator, adayların teknik mülakat pratiğini daha gerçekçi hale getirmek; eksiklerini ölçmek ve kişiselleştirilmiş gelişim adımları sunmak için tasarlanmıştır. Hem modern full-stack mimariyi hem de üretken yapay zekanın kullanıcı deneyimine güvenli biçimde entegrasyonunu gösteren portföy odaklı bir projedir.
