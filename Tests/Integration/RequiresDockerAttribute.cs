using System.Diagnostics;

using TUnit.Core;

namespace DioRed.Vermilion.Tests.Integration;

public sealed class RequiresDockerAttribute()
    : SkipAttribute("Docker daemon is not available on this machine.")
{
    public override async Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        try
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
            {
                return true;
            }

            await process.WaitForExitAsync().ConfigureAwait(false);
            return process.ExitCode != 0;
        }
        catch
        {
            return true;
        }
    }
}
