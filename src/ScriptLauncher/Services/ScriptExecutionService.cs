using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScriptLauncher.Services;

/// <summary>
/// Executes scripts with parameter passing.
/// </summary>
public class ScriptExecutionService : IScriptExecutionService
{
    private readonly ILogger<ScriptExecutionService> _logger;

    public ScriptExecutionService(ILogger<ScriptExecutionService> logger)
    {
        _logger = logger;
    }

    public async Task<ScriptExecutionResult> ExecuteAsync(
        ScriptDefinition script,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = new ScriptExecutionResult
        {
            ScriptId = script.Id,
            StartTime = DateTime.UtcNow
        };

        try
        {
            var processInfo = BuildProcessInfo(script, parameters);
            _logger.LogInformation("Executing script: {ScriptId} with {ParamCount} parameters",
                script.Id, parameters.Count);
            _logger.LogDebug("Command: {FileName} {Arguments}",
                processInfo.FileName, processInfo.Arguments);

            using var process = new Process { StartInfo = processInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeout = script.Script.Timeout ?? 30000;
            var completed = await WaitForExitAsync(process, timeout, cancellationToken);

            result.EndTime = DateTime.UtcNow;
            result.Output = outputBuilder.ToString();
            result.ErrorOutput = errorBuilder.ToString();

            if (!completed)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch { }

                result.Success = false;
                result.ErrorMessage = "Script execution timed out";
                _logger.LogWarning("Script timed out: {ScriptId}", script.Id);
            }
            else
            {
                result.ExitCode = process.ExitCode;
                result.Success = process.ExitCode == 0;

                if (result.Success)
                {
                    _logger.LogInformation("Script completed successfully: {ScriptId}", script.Id);
                }
                else
                {
                    _logger.LogWarning("Script failed with exit code {ExitCode}: {ScriptId}",
                        process.ExitCode, script.Id);
                }
            }

            // Try to parse JSON output
            result.ParsedOutput = TryParseJsonOutput(result.Output);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            _logger.LogError(ex, "Script execution failed: {ScriptId}", script.Id);
        }

        return result;
    }

    private ProcessStartInfo BuildProcessInfo(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        var (executable, arguments) = script.Script.Type.ToLowerInvariant() switch
        {
            "powershell" or "pwsh" => BuildPowerShellCommand(script, parameters),
            "batch" or "cmd" => BuildBatchCommand(script, parameters),
            "bash" or "shell" or "sh" => BuildBashCommand(script, parameters),
            "python" or "py" => BuildPythonCommand(script, parameters),
            "node" or "javascript" or "js" => BuildNodeCommand(script, parameters),
            "executable" or "exe" => BuildExecutableCommand(script, parameters),
            _ => throw new NotSupportedException($"Script type '{script.Script.Type}' is not supported")
        };

        var workingDirectory = ResolveWorkingDirectory(script);

        return new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
    }

    private (string executable, string arguments) BuildPowerShellCommand(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        // Use pwsh (PowerShell Core) for cross-platform, fallback to powershell on Windows
        var executable = OperatingSystem.IsWindows() ? "powershell" : "pwsh";
        var scriptPath = ResolvePath(script.Script.Path);

        var args = new StringBuilder();
        args.Append($"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{scriptPath}\"");

        foreach (var (key, value) in parameters)
        {
            if (value == null) continue;
            var escapedValue = EscapePowerShellValue(value);
            args.Append($" -{key} {escapedValue}");
        }
        Console.WriteLine((executable, args.ToString()));
        return (executable, args.ToString());
    }

    private (string executable, string arguments) BuildBatchCommand(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        var scriptPath = ResolvePath(script.Script.Path);
        var args = new StringBuilder();
        args.Append($"/C \"{scriptPath}\"");

        foreach (var (key, value) in parameters)
        {
            if (value == null) continue;
            args.Append($" \"{value}\"");
        }

        return ("cmd.exe", args.ToString());
    }

    private (string executable, string arguments) BuildBashCommand(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        var scriptPath = ResolvePath(script.Script.Path);
        var args = new StringBuilder();
        args.Append($"\"{scriptPath}\"");

        foreach (var (key, value) in parameters)
        {
            if (value == null) continue;
            args.Append($" \"{EscapeBashValue(value)}\"");
        }

        return ("/bin/bash", $"-c {args}");
    }

    private (string executable, string arguments) BuildPythonCommand(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        var scriptPath = ResolvePath(script.Script.Path);
        var args = new StringBuilder();
        args.Append($"\"{scriptPath}\"");

        foreach (var (key, value) in parameters)
        {
            if (value == null) continue;
            args.Append($" --{key}=\"{value}\"");
        }

        return ("python", args.ToString());
    }

    private (string executable, string arguments) BuildNodeCommand(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        var scriptPath = ResolvePath(script.Script.Path);
        var args = new StringBuilder();
        args.Append($"\"{scriptPath}\"");

        // Pass parameters as JSON via environment or args
        var paramsJson = JsonSerializer.Serialize(parameters);
        args.Append($" --params=\"{paramsJson.Replace("\"", "\\\"")}\"");

        return ("node", args.ToString());
    }

    private (string executable, string arguments) BuildExecutableCommand(
        ScriptDefinition script,
        Dictionary<string, object?> parameters)
    {
        var executablePath = ResolvePath(script.Script.Path);
        var args = new StringBuilder();

        foreach (var (key, value) in parameters)
        {
            if (value == null) continue;
            args.Append($" --{key}=\"{value}\"");
        }

        return (executablePath, args.ToString().TrimStart());
    }

    private string ResolvePath(string path)
    {
        // Handle relative paths
        if (!Path.IsPathRooted(path))
        {
            var basePath = Path.Combine(FileSystem.AppDataDirectory, "Configuration", "scripts");
            path = Path.Combine(basePath, path);
        }

        // Expand environment variables
        path = Environment.ExpandEnvironmentVariables(path);

        return path;
    }

    private string ResolveWorkingDirectory(ScriptDefinition script)
    {
        if (!string.IsNullOrEmpty(script.Script.WorkingDirectory))
        {
            return Environment.ExpandEnvironmentVariables(script.Script.WorkingDirectory);
        }

        // Default to the script's directory
        var scriptPath = ResolvePath(script.Script.Path);
        return Path.GetDirectoryName(scriptPath) ?? FileSystem.AppDataDirectory;
    }

    private static string EscapePowerShellValue(object value)
    {
        return value switch
        {
            bool b => b ? "$true" : "$false",
            DateTime dt => $"\"{dt:O}\"",
            IEnumerable<string> list => $"@({string.Join(",", list.Select(s => $"\"{s}\""))})",
            _ => $"\"{value.ToString()?.Replace("\"", "`\"")}\""
        };
    }

    private static string EscapeBashValue(object value)
    {
        var str = value.ToString() ?? "";
        return str.Replace("'", "'\\''");
    }

    private static async Task<bool> WaitForExitAsync(
        Process process,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);

        try
        {
            await process.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private static Dictionary<string, object>? TryParseJsonOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output)) return null;

        // Try to find JSON in the output (look for { ... } pattern)
        var trimmed = output.Trim();
        if (!trimmed.StartsWith("{")) return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(trimmed);
        }
        catch
        {
            return null;
        }
    }
}
