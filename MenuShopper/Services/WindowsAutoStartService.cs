using Microsoft.Win32;

namespace MenuShopper.Services;

public class WindowsAutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "MenuShopper";

    public bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(ValueName) as string;
        return !string.IsNullOrWhiteSpace(value);
    }

    public void Enable()
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException("Windows autostart is only supported on Windows.");

        var command = BuildRunCommand();
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                      ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        if (key == null)
            throw new InvalidOperationException($"Unable to open or create registry key HKCU\\{RunKeyPath}.");

        key.SetValue(ValueName, command);
    }

    public void Disable()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key == null)
            return;

        key.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    private static string BuildRunCommand()
    {
        var appHostPath = ResolveAppHostExePath();
        return $"\"{appHostPath}\"";
    }

    private static string ResolveAppHostExePath()
    {
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath)
            && Path.GetFileName(processPath).Equals("MenuShopper.exe", StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        var appHostPath = Path.Combine(AppContext.BaseDirectory, "MenuShopper.exe");
        if (File.Exists(appHostPath))
            return appHostPath;

        throw new FileNotFoundException(
            "MenuShopper.exe could not be found to enable Windows autostart.",
            appHostPath
        );
    }
}

