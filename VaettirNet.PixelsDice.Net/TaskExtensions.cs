using System;
using System.Threading;
using System.Threading.Tasks;

namespace VaettirNet.PixelsDice.Net;

internal static class TaskExtensions
{
    public static async Task IgnoreCancellation(this Task t, CancellationToken token)
    {
        try
        {
            await t;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Ignore
        }
    }
}