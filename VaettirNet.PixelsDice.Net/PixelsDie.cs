using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VaettirNet.Btleplug;
using VaettirNet.PixelsDice.Net.Animations;
using VaettirNet.PixelsDice.Net.Messages;

namespace VaettirNet.PixelsDice.Net;

public delegate void RollStateChanged(PixelsDie source, RollState state, int value, int index);

/// <summary>
/// A single pixels die. Returned by <see cref="PixelsManager.StartScan"/>.
/// </summary>
public sealed class PixelsDie : IDisposable, IAsyncDisposable
{
    private readonly BtlePeripheral _ble;

    public int LedCount { get; private set; }
    public DieType Type { get; private set; }
    public Colorway Colorway { get; private set; }
    public uint PixelId { get; private set; }
    public DateTimeOffset BuildTimestamp { get; private set; }
    public RollState RollState { get; private set; }
    public int CurrentFace { get; private set; }
    public int BatteryLevel { get; private set; }
    public int BatteryState { get; private set; }

    public ConnectionState ConnectionState { get; private set; }
    public bool IsConnected => ConnectionState == ConnectionState.Connected;

    public event Action<PixelsDie, ConnectionState> ConnectionStateChanged;
    public event Action<PixelsDie, ushort> RemoteAction;

    private ICommonProtocolHandler _commonProtocol;
    private IAnimationProtocolHandler _animationProtocol;

    /// <summary>
    /// Event triggered when a die is handled or rolled.
    /// </summary>
    public event RollStateChanged RollStateChanged;

    private PixelsDie(BtlePeripheral ble)
    {
        _ble = ble;
        ble.Disconnected += Disconnected;
    }

    private readonly object _disconnectLock = new();
    private bool _shouldTryReconnect;
    private Task _reconnectTask;
    private CancellationTokenSource _reconnectCancel;
    private void Disconnected(BtlePeripheral obj)
    {
        if (!_shouldTryReconnect || _reconnectTask != null)
        {
            SetConnectionState(ConnectionState.Disconnected);
            return;
        }

        lock (_disconnectLock)
        {
            if (!_shouldTryReconnect || _reconnectTask != null)
            {
                SetConnectionState(ConnectionState.Disconnected);
                return;
            }

            Logger.Instance.Log(PixelsLogLevel.Info, $"Lost device {obj.Address}, attempting reconnect");
            SetConnectionState(ConnectionState.Reconnecting);
            _reconnectCancel = new CancellationTokenSource();
            _reconnectTask = Task.Run(() => AttemptReconnect(_reconnectCancel.Token));
        }
    }

    private void SetConnectionState(ConnectionState conn)
    {
        ConnectionState = conn;
        ConnectionStateChanged?.Invoke(this, conn);
    }

    private async Task AttemptReconnect(CancellationToken cancellationToken)
    {
        int delaySeconds = 1;
        while (true)
        {
            try
            {
                if (await _ble.IsConnectedAsync())
                {
                    SetConnectionState(ConnectionState.Connected);
                    return;
                }

                await _ble.ConnectAsync();
                Logger.Instance.Log(PixelsLogLevel.Info, $"Device {_ble.Address} reconnected");
            }
            catch
            {
                Logger.Instance.Log(PixelsLogLevel.Info, $"Could not reconnect {_ble.Address}, trying again in {delaySeconds} seconds");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                delaySeconds = int.Min(10, delaySeconds + 1);
            }
        }
    }

    /// <summary>
    /// Connect to the device, which will cause RollStateChange events to begin
    /// triggering as well as populate additional fields.
    /// </summary>
    public async Task ConnectAsync()
    {
        _shouldTryReconnect = true;
        await _ble.ConnectAsync().ConfigureAwait(false);
        // If we don't scan for services, the registration will fail, claiming it's not a valid registration
        var services = await _ble.GetServicesAsync();
        await _ble.RegisterNotificationCallback(PixelsId.PixelsServiceUuid, PixelsId.NotifyCharacteristicUuid, DataReceived);
        await SendMessage(new WhoAmIMessage());
        await _idReceived.Task.ConfigureAwait(false);
    }
    
    /// <summary>
    /// Get an identifier that can be passed to <see cref="PixelsManager.StartScan"/> in order to save a device as
    /// connected
    /// </summary>
    public string GetPersistentIdentifier()
    {
        return _ble.Address.ToString("X");
    }

    /// <summary>
    /// Blink the die
    /// </summary>
    /// <param name="count">Number of blinks</param>
    /// <param name="duration">Total duration of entire animation</param>
    /// <param name="color">Color to blink the die</param>
    /// <param name="faceMask">Which faces to blink</param>
    /// <param name="fade">Fade strength (0 = no fade, 1.0 = maximum fade)</param>
    /// <param name="loop">True to repeat the blinking animation</param>
    public Task BlinkAsync(int count, TimeSpan duration, Color color, int faceMask, double fade, bool loop)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfLessThan(fade, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fade, 1);
        
        var msg = new BlinkMessage
        {
            Color = color.ToArgb(),
            Count = (byte)count,
            Faces = faceMask,
            Fade = (byte)(fade * 255),
            Loop = loop,
            DurationMs = (short)duration.TotalMilliseconds
        };
        return SendMessage(msg);
    }

    private async Task SendMessage<T>(T msg) where T : struct
    {
        await _ble.Write(
            PixelsId.PixelsServiceUuid,
            PixelsId.WriteCharacteristicUuid,
            GetBuffer(msg),
            true
        );

        byte[] GetBuffer(T m)
        {
            var span = MemoryMarshal.Cast<T, byte>(new Span<T>(ref m));
            var b = new byte[span.Length];
            span.CopyTo(b);
            return b;
        }
    }

    public Task SendInstantAnimations(InstantAnimationSet instantAnimations)
    {
        return _animationProtocol.SendInstantAnimationsAsync(this, instantAnimations);
    }
    
    public Task SendAnimationSet(AnimationSet animationSet)
    {
        return _animationProtocol.SendAnimationSetAsync(this, animationSet);
    }

    public Task PlayInstantAnimationAsync(int index, int loopCount, byte faceIndex)
    {
        return _animationProtocol.PlayInstantAnimationAsync(this, index, loopCount, faceIndex);
    }

    public Task StopAllAnimationsAsync()
    {
        return SendMessage(new GenericMessage(MessageType.StopAllAnims));
    }

    private Task<TAck> SendAndWaitForAck<TMessage, TAck>(TMessage message, MessageType ackType)
        where TMessage : struct
        where TAck : struct 
    {
        var src = new TaskCompletionSource<object>();
        _ackHandlers[ackType] = (src, msg => MemoryMarshal.Read<TAck>(msg));
        return Wait();
        async Task<TAck> Wait()
        {
            await SendMessage(message);
            var ret = (TAck)await src.Task;
            _ackHandlers.Remove(ackType);
            return ret;
        }
    }
    
    private Task<TAck> WaitForMessage<TAck>(MessageType ackType) where TAck : struct
    {
        var src = new TaskCompletionSource<object>();
        _ackHandlers[ackType] = (src, msg => MemoryMarshal.Read<TAck>(msg));
        return Wait();
        async Task<TAck> Wait()
        {
            var ret = (TAck)await src.Task;
            _ackHandlers.Remove(ackType);
            return ret;
        }
    }

    private readonly TaskCompletionSource _idReceived = new();
    private delegate object SerializeAck(ReadOnlySpan<byte> data);
    private readonly Dictionary<MessageType, (TaskCompletionSource<object> recieved, SerializeAck serialize)> _ackHandlers = [];
    
    private void DataReceived(BtlePeripheral peripheral, Guid service, Guid characteristic, Span<byte> msg)
    {
        if (msg.Length == 0)
            return;

        var messageType = (MessageType)msg[0];
        switch (messageType)
        {
            case MessageType.IAmADie:
                HandleIAmADie(msg);
                break;
            case MessageType.RollState:
                HandleDieRoll(MemoryMarshal.Read<RollStateMessage>(msg));
                break;
            case MessageType.RemoteAction:
            {
                Action<PixelsDie, ushort> action = RemoteAction;
                if (action != null)
                {
                    var remoteActionMessage = MemoryMarshal.Read<RemoteActionMessage>(msg);
                    action(this, remoteActionMessage.ActionId);
                }
                break;
            }
            case MessageType.BatteryLevel:
                HandleBatteryMessage(MemoryMarshal.Read<BatteryLevelMessage>(msg));
                break;
            default:
                if (_ackHandlers.TryGetValue(messageType, out var handler))
                {
                    object value = handler.serialize(msg);
                    handler.recieved.TrySetResult(value);
                    break;
                }

                // Messages we don't care about
                break;
        }
    }

    private void HandleBatteryMessage(BatteryLevelMessage msg)
    {
        BatteryLevel = msg.BatteryLevel;
        BatteryState = msg.BatteryState;
    }

    private void HandleDieRoll(RollStateMessage msg)
    {
        CurrentFace = msg.CurrentFace;
        RollState = msg.RollState;
        RollStateChanged?.Invoke(this, msg.RollState, GetFaceValue(Type, msg.CurrentFace), msg.CurrentFace);
    }

    public int GetFaceValue(int value) => GetFaceValue(Type, value);

    public static int GetFaceValue(DieType dieType, int value)
    {
        return dieType switch
        {
            DieType.FD6 => value switch
            {
                2 or 5 => -1,
                1 or 6 => 1,
                _ => 0,
            },
            DieType.D00 => value * 10,
            _ => value + 1,
        };
    }
    
    public int GetFaceIndex(int value) => GetFaceValue(Type, value);
    
    public static int GetFaceIndex(DieType dieType, int value)
    {
        return dieType switch
        {
            DieType.FD6 => value switch
            {
                1 => 6,
                0 => 4,
                -1 => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            },
            DieType.D00 => value / 10,
            _ => value - 1,
        };
    }

    private void HandleIAmADie(ReadOnlySpan<byte> msg)
    {
        if (_commonProtocol != null)
        {
            return;
        }

        if (msg.Length == 22)
        {
            _commonProtocol = new PrereleaseCommonProtocol();
            _animationProtocol = new PrereleaseAnimationProtocolHandler();
        }

        if (_commonProtocol == null)
        {
            throw new NotSupportedException(
                "Die firmware is using an unsupported protocol version, please update this library.");
        }

        _commonProtocol.IAmADie(this, msg);
        _idReceived.TrySetResult();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (ConnectionState == ConnectionState.Connected)
            await _ble.DisconnectAsync();
        _ble.Dispose();
    }

    internal static PixelsDie Create(BtlePeripheral ble)
    {
        return new PixelsDie(ble);
    }

    private static async Task SendBulkDataAsync(PixelsDie die, byte[] buffer)
    {
        await die.SendAndWaitForAck<BulkSetupMessage, GenericMessage>(
            new BulkSetupMessage { Size = (ushort)buffer.Length },
            MessageType.BulkSetupAck);

        Memory<byte> remBuffer = buffer.AsMemory();
        int offset = 0;
        while (!remBuffer.IsEmpty)
        {
            byte len = (byte)Math.Min(100, remBuffer.Length);
            Memory<byte> toSend = remBuffer[..len];
            var msg = new BulkDataMessage { Offset = (ushort)offset, Size = len };
            unsafe
            {
                toSend.Span.CopyTo(new Span<byte>(msg.Data, 100));
            }

            _ = await die.SendAndWaitForAck<BulkDataMessage, BulkDataAckMessage>(msg, MessageType.BulkDataAck);
            offset += len;
            remBuffer = remBuffer[len..];
        }
    }

    private interface ICommonProtocolHandler
    {
        void IAmADie(PixelsDie pixelsDie, ReadOnlySpan<byte> msg);
    }

    private class PrereleaseCommonProtocol : ICommonProtocolHandler
    {
        public void IAmADie(PixelsDie pixelsDie, ReadOnlySpan<byte> msg)
        {
            var id = MemoryMarshal.Read<PrereleaseIAmADieMessage>(msg);
            
            pixelsDie.PixelId = id.PixelId;
            pixelsDie.LedCount = id.LedCount;
            pixelsDie.Type = id.Type;
            pixelsDie.Colorway = id.Colorway;
            pixelsDie.BuildTimestamp = DateTimeOffset.FromUnixTimeSeconds(id.BuildTimestamp);
            pixelsDie.RollState = id.RollState;
            pixelsDie.CurrentFace = id.CurrentFace;
            pixelsDie.BatteryLevel = id.BatteryLevel;
            pixelsDie.BatteryState = id.BatteryState;
        }
    }

    private interface IAnimationProtocolHandler
    {
        Task PlayInstantAnimationAsync(PixelsDie die, int index, int loopCount, byte faceIndex);
        Task SendInstantAnimationsAsync(PixelsDie die, InstantAnimationSet instantAnimations);
        Task SendAnimationSetAsync(PixelsDie die, AnimationSet animationSet);
    }

    private class PrereleaseAnimationProtocolHandler : IAnimationProtocolHandler
    {
        public Task PlayInstantAnimationAsync(PixelsDie die, int index, int loopCount, byte faceIndex)
        {
            return die.SendMessage(new PlayInstantAnimationMessage
                { Animation = (byte)index, LoopCount = 0, FaceIndex = faceIndex });
        }

        public Task SendInstantAnimationsAsync(PixelsDie die, InstantAnimationSet instantAnimations)
        {
            SerializedAnimationData serialized = instantAnimations.Serialize();

            uint hash = AnimationUtils.Hash(serialized.Buffer);
            Logger.Instance.Log(PixelsLogLevel.Info, $"Sending hash {hash}");
            var animSetMessage = new TransferInstantAnimSetMessage
            {
                PaletteSize = (ushort)(serialized.Data.Palette.Count * 3),
                RgbKeyFrameCount = (ushort)serialized.Data.RgbKeyFrames.Count,
                RgbTrackCount = (ushort)serialized.Data.RgbTracks.Count,
                KeyFrameCount = (ushort)serialized.Data.KeyFrames.Count,
                TrackCount = (ushort)serialized.Data.Tracks.Count,
                AnimationCount = (ushort)serialized.Data.AnimationBuffer.Count,
                AnimationSize = (ushort)serialized.Data.AnimationBuffer.Size,
                Hash = hash,
            };
            byte[] buffer = serialized.Buffer;
            
            return SendAnimations();

            async Task SendAnimations()
            {
                TransferInstantAnimSetAckMessage ack =
                    await die.SendAndWaitForAck<TransferInstantAnimSetMessage, TransferInstantAnimSetAckMessage>(
                        animSetMessage,
                        MessageType.TransferInstantAnimSetAck);

                switch (ack.Type)
                {
                    case TransferInstantAnimSetAckType.NoMemory:
                        throw new DeviceOutOfMemoryException("Device reported no memory for animation set");
                    case TransferInstantAnimSetAckType.UpToDate:
                        return;
                }
                
                Task<GenericMessage> finishAck = die.WaitForMessage<GenericMessage>(MessageType.TransferInstantAnimSetFinished);
                Task bulkData = SendBulkDataAsync(die, buffer);

                await Task.WhenAll(finishAck, bulkData);
            }
        }

        public Task SendAnimationSetAsync(PixelsDie die, AnimationSet animationSet)
        {
            SerializedAnimationData serialized = animationSet.Serialize();

            uint hash = AnimationUtils.Hash(serialized.Buffer);
            Logger.Instance.Log(PixelsLogLevel.Info, $"Sending hash {hash}");
            var animSetMessage = new TransferAnimSetMessage()
            {
                PaletteSize = (ushort)(serialized.Data.Palette.Count * 3),
                RgbKeyFrameCount = (ushort)serialized.Data.RgbKeyFrames.Count,
                RgbTrackCount = (ushort)serialized.Data.RgbTracks.Count,
                KeyFrameCount = (ushort)serialized.Data.KeyFrames.Count,
                TrackCount = (ushort)serialized.Data.Tracks.Count,
                AnimationCount = (ushort)serialized.Data.AnimationBuffer.Count,
                AnimationSize = (ushort)serialized.Data.AnimationBuffer.Size,
                ActionCount = (ushort)serialized.Data.ActionBuffer.Count,
                ActionSize = (ushort)serialized.Data.ActionBuffer.Size,
                ConditionCount = (ushort)serialized.Data.ConditionBuffer.Count,
                ConditionSize = (ushort)serialized.Data.ConditionBuffer.Size,
                RuleCount = (ushort)serialized.Data.Rules.Count
            };
            
            byte[] buffer = serialized.Buffer;
            
            return SendAnimationsAsync();

            async Task SendAnimationsAsync()
            {
                var ack = await die.SendAndWaitForAck<TransferAnimSetMessage, TransferAnimSetAckMessage>(
                        animSetMessage,
                        MessageType.TransferAnimSetAck);

                if (ack.Result == 0)
                {
                    Logger.Instance.Log(PixelsLogLevel.Error, "Die refused animation set");
                    return;
                }

                Task<GenericMessage> finish = die.WaitForMessage<GenericMessage>(MessageType.TransferAnimSetFinished);
                
                Task bulkData = SendBulkDataAsync(die, buffer);

                List<Task> waitingTask = [finish, bulkData];
                while (waitingTask.Count > 0)
                {
                    var comp = await Task.WhenAny(waitingTask);
                    waitingTask.Remove(comp);
                    if (comp == finish)
                    {
                        var res = await finish;
                    }
                    else if (comp == bulkData)
                    {
                    }
                }
            }
        }
    }
}