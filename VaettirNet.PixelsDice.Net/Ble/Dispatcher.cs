using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace VaettirNet.PixelsDice.Net.Ble;

/// <summary>
/// The SimpleBLE library appears to have some thread affinity. In particular,
/// connecting to the dice from an STA thread (e.g. WPF) doesn't work.  So this dispatcher
/// will run a processing thread will force itself into MTA when necessary
/// and shuttle the values back and forth to allow usage from other threads.
/// </summary>
internal class Dispatcher
{
    private readonly object _prepareLock = new();
    private ConcurrentQueue<(Action action, TaskCompletionSource complete)> _queue;
    private AutoResetEvent _readyEvent;

    internal Task Execute(Action action)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
        {
            // If we are already in an MTA thread, we can just run the code directly, save ourselves a dispatch call.
            action();
            return Task.CompletedTask;
        }

        return SendToDispatcher(action);
    }

    private Task SendToDispatcher(Action action)
    {
        PrepareBackgroundThread();

        TaskCompletionSource c = new();
        _queue.Enqueue((action, c));
        _readyEvent.Set();
        return c.Task;
    }

    private void PrepareBackgroundThread()
    {
        if (_queue != null) return;
        
        lock (_prepareLock)
        {
            if (_queue != null) return;
            
            _queue = new ConcurrentQueue<(Action action, TaskCompletionSource complete)>();
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
            if (_queue.TryDequeue(out (Action action, TaskCompletionSource complete) item))
            {
                try
                {
                    item.action();
                    item.complete.SetResult();
                }
                catch (Exception e)
                {
                    item.complete.SetException(e);
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