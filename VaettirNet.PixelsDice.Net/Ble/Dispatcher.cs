using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace VaettirNet.PixelsDice.Net.Ble;

internal record struct DispatchRecord(Func<object> Action, TaskCompletionSource<object> Complete, Stopwatch Delay);

/// <summary>
/// The SimpleBLE library appears to have some thread affinity. In particular,
/// connecting to the dice from an STA thread (e.g. WPF) doesn't work.  So this dispatcher
/// will run a processing thread will force itself into MTA when necessary
/// and shuttle the values back and forth to allow usage from other threads.
/// </summary>
internal class Dispatcher
{
    private readonly object _prepareLock = new();
    private ConcurrentQueue<DispatchRecord> _queue;
    private AutoResetEvent _readyEvent;

    internal Task Execute(Action action)
    {
        if (IsDispatchRequired)
        {
            // If we are already in an MTA thread, we can just run the code directly, save ourselves a dispatch call.
            action();
            return Task.CompletedTask;
        }

        return SendToDispatcher(() =>
        {
            action();
            return null;
        });
    }

    public bool IsDispatchRequired => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA;

    internal async Task<T> Execute<T>(Func<T> func)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
        {
            // If we are already in an MTA thread, we can just run the code directly, save ourselves a dispatch call.
            return func();
        }

        return (T) await SendToDispatcher(() => func());
    }

    private Task<object> SendToDispatcher(Func<object> handler)
    {
        PrepareBackgroundThread();

        TaskCompletionSource<object> c = new();
        _queue.Enqueue(new(handler, c, Stopwatch.StartNew()));
        _readyEvent.Set();
        return c.Task;
    }

    private void PrepareBackgroundThread()
    {
        if (_queue != null) return;
        
        lock (_prepareLock)
        {
            if (_queue != null) return;
            
            Logger.Instance.Log(PixelsLogLevel.Info, "Initiating background dispatcher thread to handle thread apartments");
            _queue = new ConcurrentQueue<DispatchRecord>();
            _readyEvent = new AutoResetEvent(false);
            var t = new Thread(ExecuteQueue)
            {
                IsBackground = true
            };
            t.Start();
        }
    }

    [DoesNotReturn]
    private void ExecuteQueue()
    {
        while(true)
        {
            if (_queue.TryDequeue(out DispatchRecord item))
            {
                try
                {
                    item.Complete.SetResult(item.Action());
                }
                catch (Exception e)
                {
                    item.Complete.SetException(e);
                }
            }
            else
            {
                // There was nothing there, so wait until the even pulses, because
                // the enqueuing thread always pulses the event after pushing
                _readyEvent.WaitOne();
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}