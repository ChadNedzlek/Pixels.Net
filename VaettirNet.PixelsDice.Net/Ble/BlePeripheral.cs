using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VaettirNet.PixelsDice.Net.Interop;

namespace VaettirNet.PixelsDice.Net.Ble;

internal sealed class BlePeripheral : IDisposable, IAsyncDisposable
{
    public string Id { get; }
    public string Address { get; }
    private readonly SafePeripheralHandle _handle;
    private readonly Dispatcher _dispatcher;

    private NotifyCallback _notifyCallback;
    private OnNotifyCallback _receiveCallback;

    private readonly object _reconnectLock = new();
    private ConnectionCallback _disconnectedCallback;
    private ConnectionCallback _connectedCallback;
    private Task _reconnectTask;
    private CancellationTokenSource _reconnectCancellation;

    public ConnectionState ConnectionState;
    private AsyncAutoResetEvent _disconnectedEvent;

    public bool IsConnected => ConnectionState == ConnectionState.Connected;

    public event Action<BlePeripheral, ConnectionState> ConnectionStateChanged;

    public Task ConnectAsync(OnNotifyCallback receiveCallback)
    {
        if (_notifyCallback != null)
            throw new InvalidOperationException();

        if (_disconnectedCallback == null)
        {
            lock (_reconnectLock)
            {
                if (_disconnectedCallback == null)
                {
                    _disconnectedCallback = OnDisconnected;
                    _connectedCallback = OnConnected;
                    NativeMethods.OnDisconnected(_handle, _disconnectedCallback, IntPtr.Zero).CheckSuccess();
                    NativeMethods.OnConnected(_handle, _connectedCallback, IntPtr.Zero).CheckSuccess();
                }
            }
        }

        _notifyCallback = OnNotify;
        _receiveCallback = receiveCallback;

        return _dispatcher.Execute(Dispatched);

        void Dispatched()
        {
            NativeMethods.ConnectPeripheral(_handle).CheckSuccess();
            NativeMethods.OnNotify(_handle,
                    PixelsId.PixelsServiceUuid,
                    PixelsId.NotifyCharacteristicUuid,
                    _notifyCallback,
                    IntPtr.Zero)
                .CheckSuccess();
            ConnectionState = ConnectionState.Connected;
        }
    }

    private void SetConnectionState(ConnectionState connectionState)
    {
        ConnectionState = connectionState;
        ConnectionStateChanged?.Invoke(this, connectionState);
    }

    private void OnConnected(IntPtr peripheral, IntPtr userdata)
    {
        SetConnectionState(ConnectionState.Connected);
    }

    private void OnDisconnected(IntPtr peripheral, IntPtr userdata)
    {
        Logger.Instance.Log(PixelsLogLevel.Info, $"Connection to device (id: {Id}, addr: {Address}) lost... reconnecting...");
        SetConnectionState(ConnectionState.Reconnecting);
        lock (_reconnectLock)
        {
            if (_reconnectTask == null)
            {
                _reconnectCancellation = new CancellationTokenSource();
                _disconnectedEvent = new AsyncAutoResetEvent(false);
                _reconnectTask = Task.Factory.StartNew(() => ReconnectCallback(_reconnectCancellation.Token),
                    _reconnectCancellation.Token);
            }
            else
            {
                _disconnectedEvent.Set();
            }
        }
    }

    private async Task ReconnectCallback(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            CallResult result = NativeMethods.IsConnectable(_handle, out bool connectable);

            if (result == CallResult.Failure || connectable == false)
            {
                // Die is not connectable, just wait for a bit
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                continue;
            }

            result = await _dispatcher.Execute(() => NativeMethods.ConnectPeripheral(_handle));
            switch (result)
            {
                case CallResult.Success:
                    // We are reconnected, wait until we get disconnected again
                    result = NativeMethods.IsConnected(_handle, out bool connected);
                    if (result == CallResult.Failure || connected == false)
                    {
                        // We didn't really connect, try again
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                        continue;
                    }
                    
                    Logger.Instance.Log(PixelsLogLevel.Info, $"Device (id: {Id}, addr: {Address}) reconnected");

                    await _disconnectedEvent.WaitAsync(cancellationToken);
                    break;
                case CallResult.Failure:
                    // That didn't work, just chill for a bit
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    break;
            }
        }
    }

    private void OnNotify(BleUuid service, BleUuid characteristic, IntPtr data, UIntPtr dataLength, IntPtr userdata)
    {
        byte[] buffer = new byte[dataLength];
        Marshal.Copy(data, buffer, 0, (int)dataLength);
        unsafe
        {
            _receiveCallback(new Span<byte>(data.ToPointer(), (int)dataLength));
        }
    }

    public void SendMessage<T>(T data) where T : struct
    {
        Span<byte> buffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref data, 1));
        NativeMethods.WriteCommand(
                _handle,
                PixelsId.PixelsServiceUuid,
                PixelsId.WriteCharacteristicUuid,
                ref buffer[0],
                (UIntPtr)buffer.Length
            )
            .CheckSuccess();
    }

    private async Task StopReconnectionAsync()
    {
        if (_disconnectedCallback == null)
            return;

        Task runningTask;

        lock (_reconnectLock)
        {
            if (_disconnectedCallback == null)
                return;

            runningTask = Interlocked.Exchange(ref _reconnectTask, null);
            _reconnectCancellation.Cancel();
        }

        if (runningTask != null)
        {
            await _reconnectTask.IgnoreCancellation(_reconnectCancellation.Token);
        }
    }

    public async Task DisconnectAsync()
    {
        await StopReconnectionAsync();
        await _dispatcher.Execute(() => { NativeMethods.DisconnectPeripheral(_handle).CheckSuccess(); }).ConfigureAwait(false);
        _notifyCallback = null;
        ConnectionState = ConnectionState.Disconnected;
    }

    public static BlePeripheral Create(SafePeripheralHandle handle, Dispatcher dispatcher)
    {
        using StringHandle id = NativeMethods.GetPeripheralIdentifier(handle);
        using StringHandle addy = NativeMethods.GetPeripheralAddress(handle);
        return new BlePeripheral(handle, id.Value, addy.Value, dispatcher);
    }

    public string GetPersistentId()
    {
        return GetPersistentId(Id, Address);
    }

    public static string GetPersistentId(SafePeripheralHandle bleHandle)
    {
        using StringHandle id = NativeMethods.GetPeripheralIdentifier(bleHandle);
        using StringHandle addr = NativeMethods.GetPeripheralAddress(bleHandle);
        return GetPersistentId(id.Value, addr.Value);
    }

    private static string GetPersistentId(string id, string address)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
        {
            writer.Write(id);
            writer.Write(address);
        }

        return Convert.ToBase64String(stream.ToArray());
    }

    public byte[][] GetManufacturerData()
    {
        nuint cnt = NativeMethods.GetManufacturerDataCount(_handle);
        if (cnt == 0)
            return Array.Empty<byte[]>();
        var ret = new byte[cnt][];
        var data = new BleManufacturerData();
        for (nuint i = 0; i < cnt; i++)
        {
            NativeMethods.GetManufacturerData(_handle, i, ref data);
            ret[i] = new byte[data.DataLength];
            Array.Copy(data.Data, 0, ret[i], 0, (int)data.DataLength);
        }

        return ret;
    }

    private BlePeripheral(SafePeripheralHandle handle, string id, string address, Dispatcher dispatcher)
    {
        Id = id;
        Address = address;
        _handle = handle;
        _dispatcher = dispatcher;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await StopReconnectionAsync();
        
        if (Interlocked.Exchange(ref _notifyCallback, null) != null)
        {
            await DisconnectAsync();
        }

        _handle.Dispose();
    }
}