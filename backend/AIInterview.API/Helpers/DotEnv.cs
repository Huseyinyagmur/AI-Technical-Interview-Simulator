namespace AIInterview.API.Helpers;

public static class DotEnv
{
    // A tiny local-only .env reader keeps secrets out of appsettings.json and source control.
    public static void Load()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, ".env");
            if (File.Exists(path))
            {
                foreach (var line in File.ReadLines(path))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
                    var separator = trimmed.IndexOf('=');
                    if (separator <= 0) continue;
                    var key = trimmed[..separator].Trim();
                    var value = trimmed[(separator + 1)..].Trim().Trim('"', '\'');
                    // A machine-level environment variable always has priority over .env.
                    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key))) Environment.SetEnvironmentVariable(key, value);
                }
                return;
            }
            directory = directory.Parent;
        }
    }
}
