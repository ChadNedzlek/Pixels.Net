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

    private BlePeripheral(SafePeripheralHandle handle, string id, string address, Dispatcher dispatcher)
    {
        Id = id;
        Address = address;
        _handle = handle;
        _dispatcher = dispatcher;
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _notifyCallback, null) != null)
        {
            await DisconnectAsync();
        }
        _handle.Dispose();
    }

    private NotifyCallback _notifyCallback;
    private GCHandle _callbackHandle;
    private OnNotifyCallback _receiveCallback;

    public bool IsConnected => _notifyCallback != null;

    public Task ConnectAsync(OnNotifyCallback receiveCallback)
    {
        if (_notifyCallback != null)
            throw new InvalidOperationException();
        
        _notifyCallback = OnNotify;
        _receiveCallback = receiveCallback;
        _callbackHandle = GCHandle.Alloc(_notifyCallback);

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
        var buffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref data, 1));
        NativeMethods.WriteRequest(
            _handle,
            PixelsId.PixelsServiceUuid,
            PixelsId.WriteCharacteristicUuid,
            ref buffer[0],
            (UIntPtr)buffer.Length
        ).CheckSuccess();
    }

    public async Task DisconnectAsync()
    {
        await _dispatcher.Execute(() => { NativeMethods.DisconnectPeripheral(_handle).CheckSuccess(); }).ConfigureAwait(false);
        _notifyCallback = null;
        _callbackHandle.Free();
    }

    public static BlePeripheral Create(SafePeripheralHandle handle, Dispatcher dispatcher)
    {
        using var id = NativeMethods.GetPeripheralIdentifier(handle);
        using var addy = NativeMethods.GetPeripheralAddress(handle);
        return new BlePeripheral(handle, id.Value, addy.Value, dispatcher);
    }

    public string GetPersistentId()
    {
        return GetPersistentId(Id, Address);
    }

    public static string GetPersistentId(SafePeripheralHandle bleHandle)
    {
        using var id = NativeMethods.GetPeripheralIdentifier(bleHandle);
        using var addr = NativeMethods.GetPeripheralAddress(bleHandle);
        return GetPersistentId(id.Value, addr.Value);
    }

    private static string GetPersistentId(string id, string address)
    {
        using MemoryStream stream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
        {
            writer.Write(id);
            writer.Write(address);
        }

        return Convert.ToBase64String(stream.ToArray());
    }

    public byte[][] GetManufacturerData()
    {
        var cnt = NativeMethods.GetManufacturerDataCount(_handle);
        if (cnt == 0)
            return Array.Empty<byte[]>();
        var ret = new byte[cnt][];
        BleManufacturerData data = new BleManufacturerData();
        for (nuint i = 0; i < cnt; i++)
        {
            NativeMethods.GetManufacturerData(_handle, i, ref data);
            ret[i] = new byte[data.DataLength];
            Array.Copy(data.Data, 0, ret[i], 0, (int)data.DataLength);
        }

        return ret;
    }
}