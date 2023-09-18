using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class ProcessExtensions
{
    public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        if (process.HasExited)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();
        process.EnableRaisingEvents = true;
        process.Exited += (sender, args) =>
        {
            tcs.TrySetResult(true);
        };
        if (cancellationToken != default)
            cancellationToken.Register(() => tcs.TrySetCanceled());

        return tcs.Task;
    }
}
