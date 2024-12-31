using System.Diagnostics;

namespace FirewallManagerApi.Services;

public interface IFirewallManagerService
{
    void AddRule(string[] remoteIPs);
    void DeleteRule();
}

public class FirewallManagerService : IFirewallManagerService
{
    private readonly ILogger<FirewallManagerService> _logger;
    private const string RuleName = "Opt Firewall Rule";

    public FirewallManagerService(ILogger<FirewallManagerService> logger)
    {
        _logger = logger;
    }

    public void AddRule(string[] remoteIPs)
    {
        const string protocol = "UDP";
        const string localPort = "50205";

        var remoteIpList = string.Join("\",\"", remoteIPs);
        remoteIpList = $"\"{remoteIpList}\"";

        var script = $@"
            New-NetFirewallRule -DisplayName '{RuleName}' `
            -Direction Inbound `
            -Action Block `
            -RemoteAddress {remoteIpList} `
            -Protocol {protocol} `
            -LocalPort {localPort} `
            -Enabled True
        ";

        RunPowerShellScript(script);
    }

    public void DeleteRule()
    {
        // Delete existing firewall rule if it exists
        const string deleteScript = $@"
            if (Get-NetFirewallRule -DisplayName '{RuleName}' -ErrorAction SilentlyContinue) {{
                Remove-NetFirewallRule -DisplayName '{RuleName}'
                Write-Output 'Existing firewall rule removed.'
            }} else {{
                Write-Output 'No existing rule found to delete.'
            }}
        ";
        RunPowerShellScript(deleteScript);
    }

    private void RunPowerShellScript(string script)
    {
        try
        {
            // Configure the PowerShell process
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas" // Ensure it's run as Administrator
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                _logger.LogError("Failed to start PowerShell process.");
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
                _logger.LogInformation($"Output: {output}");

            if (!string.IsNullOrEmpty(error))
                 _logger.LogError($"Error: {error}");

            _logger.LogInformation($"PowerShell script executed with exit code {process.ExitCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception: {ex.Message}");
        }
    }
}

