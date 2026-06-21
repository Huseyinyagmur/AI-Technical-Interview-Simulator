namespace AIInterview.API.Helpers;

public static class PromptReader
{
    // Prompts live at repository level so they can be edited without touching C# source.
    public static string Read(string fileName)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "prompts", fileName);
            if (File.Exists(candidate)) return File.ReadAllText(candidate);
            directory = directory.Parent;
        }
        return string.Empty;
    }
}
