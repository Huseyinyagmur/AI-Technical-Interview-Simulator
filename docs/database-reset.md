# Local development database reset

> Warning: these commands are for local development only. Dropping a database permanently deletes its interview history.

From `backend/AIInterview.API`, first apply all pending schema migrations:

```powershell
dotnet ef database update
```

If a local database was created with an older schema and needs a complete reset:

```powershell
dotnet ef database drop
dotnet ef database update
```

Restart the API after updating the schema. The startup console output reports pending migrations and reminds you to run `dotnet ef database update`.
