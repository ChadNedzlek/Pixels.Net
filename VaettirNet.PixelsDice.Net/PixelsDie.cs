using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VaettirNet.PixelsDice.Net.Animations;
using VaettirNet.PixelsDice.Net.Ble;
using VaettirNet.PixelsDice.Net.Messages;

namespace VaettirNet.PixelsDice.Net;

public delegate void RollStateChanged(PixelsDie source, RollState state, int value, int index);

/// <summary>
/// A single pixels die. Returned by <see cref="PixelsManager.StartScan"/>.
/// </summary>
public sealed class PixelsDie : IDisposable, IAsyncDisposable
{
    private readonly BlePeripheral _ble;

    public int LedCount { get; private set; }
    public DieType Type { get; private set; }
    public Colorway Colorway { get; private set; }
    public uint PixelId { get; private set; }
    public DateTimeOffset BuildTimestamp { get; private set; }
    public RollState RollState { get; private set; }
    public int CurrentFace { get; private set; }
    public int BatteryLevel { get; private set; }
    public int BatteryState { get; private set; }

    public bool IsConnected { get; private set; }

    /// <summary>
    /// Event triggered when a die is handled or rolled.
    /// </summary>
    public event RollStateChanged RollStateChanged;

    private PixelsDie(BlePeripheral ble)
    {
        _ble = ble;
    }

    /// <summary>
    /// Connect to the device, which will cause RollStateChange events to begin
    /// triggering as well as populate additional fields.
    /// </summary>
    public async Task ConnectAsync()
    {
        await _ble.ConnectAsync(DataReceived).ConfigureAwait(false);
        _ble.SendMessage(new WhoAmIMessage());
        IAmADieMessage id = await _idReceived.Task.ConfigureAwait(false);
        PixelId = id.PixelId;
        LedCount = id.LedCount;
        Type = id.Type;
        Colorway = id.Colorway;
        BuildTimestamp = DateTimeOffset.FromUnixTimeSeconds(id.BuildTimestamp);
        RollState = id.RollState;
        CurrentFace = id.CurrentFace;
        BatteryLevel = id.BatteryLevel;
        BatteryState = id.BatteryState;
        IsConnected = true;
    }
    
    /// <summary>
    /// Get an identifier that can be passed to <see cref="PixelsManager.StartScan"/> in order to save a device as
    /// connected
    /// </summary>
    public string GetPersistentIdentifier()
    {
        return _ble.GetPersistentId();
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
    public void Blink(int count, TimeSpan duration, Color color, int faceMask, double fade, bool loop)
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
        _ble.SendMessage(msg);
    }

    public async Task SendInstantAnimations(AnimationCollection animations)
    {
        SerializedAnimationData serialized = animations.Serialize();

        uint hash = AnimationUtils.Hash(serialized.Buffer);
        Logger.Instance.Log(PixelsLogLevel.Info, $"Sending hash {hash}");
        var animSetMessage = new TransferInstantAnimSetMessage
        {
            PaletteSize = (ushort)(serialized.Data.Palette.Count * 3),
            RgbKeyFrameCount = (ushort)serialized.Data.RgbKeyFrames.Count,
            RgbTrackCount = (ushort)serialized.Data.RgbTracks.Count,
            KeyFrameCount = (ushort)serialized.Data.KeyFrames.Count,
            TrackCount = (ushort)serialized.Data.Tracks.Count,
            AnimationCount = (ushort)animations.Animations.Count,
            AnimationSize = serialized.AnimationSize,
            Hash = hash,
        };
        TransferInstantAnimSetAckMessage ack = await SendAndWaitForAck<TransferInstantAnimSetMessage, TransferInstantAnimSetAckMessage>(
            animSetMessage,
            MessageType.TransferInstantAnimSetAck);

        switch (ack.Type)
        {
            case TransferInstantAnimSetAckType.NoMemory:
                throw new DeviceOutOfMemoryException("Device reported no memory for animation set");
            case TransferInstantAnimSetAckType.UpToDate:
                return;
        }

        await SendAndWaitForAck<BulkSetupMessage, GenericMessage>(
            new BulkSetupMessage { Size = (ushort)serialized.Buffer.Length },
            MessageType.BulkSetupAck);
        
        var finishAck = WaitForMessage<GenericMessage>(MessageType.TransferInstantAnimSetFinished);

        Memory<byte> remBuffer = serialized.Buffer.AsMemory();
        int offset = 0;
        while (!remBuffer.IsEmpty)
        {
            byte len = (byte)Math.Min(100, remBuffer.Length);
            var toSend = remBuffer[..len];
            var msg = new BulkDataMessage{Offset = (ushort)offset, Size = len};
            unsafe
            {
                toSend.Span.CopyTo(new Span<byte>(msg.Data, 100));
            }
            _ = await SendAndWaitForAck<BulkDataMessage, BulkDataAckMessage>(msg, MessageType.BulkDataAck);
            offset += len;
            remBuffer = remBuffer[len..];
        }

        await finishAck;
    }

    public void PlayInstantAnimation(int index, byte faceIndex)
    {
        // BUG: "loopCount" is recent, before that, it's a boolean, so we can't set it to anything
        // without checking the firmware version and having lots of versions of the messages
        // For now, no looping.
        _ble.SendMessage(new PlayInstantAnimationMessage
            { Animation = (byte)index, LoopCount = (byte)0, FaceIndex = faceIndex });
    }

    public void StopAllAnimations()
    {
        _ble.SendMessage(new GenericMessage(MessageType.StopAllAnims));
    }

    private Task<TAck> SendAndWaitForAck<TMessage, TAck>(TMessage message, MessageType ackType)
        where TMessage : struct
        where TAck : struct 
    {
        var src = new TaskCompletionSource<object>();
        _ackHandlers[ackType] = (src, msg => MemoryMarshal.Read<TAck>(msg));
        _ble.SendMessage(message);
        return Wait();
        async Task<TAck> Wait()
        {
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

    private readonly TaskCompletionSource<IAmADieMessage> _idReceived = new();
    private delegate object SerializeAck(ReadOnlySpan<byte> data);
    private readonly Dictionary<MessageType, (TaskCompletionSource<object> recieved, SerializeAck serialize)> _ackHandlers = [];
    
    private void DataReceived(ReadOnlySpan<byte> msg)
    {
        if (msg.Length == 0)
            return;

        var messageType = (MessageType)msg[0];
        switch (messageType)
        {
            case MessageType.IAmADie:
                HandleIAmADie(MemoryMarshal.Read<IAmADieMessage>(msg));
                break;
            case MessageType.RollState:
                HandleDieRoll(MemoryMarshal.Read<RollStateMessage>(msg));
                break;
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

    public int GetFaceValue(DieType dieType, int value)
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

    private void HandleIAmADie(IAmADieMessage msg)
    {
        if (!_idReceived.TrySetResult(msg))
        {
            Logger.Instance.Log(PixelsLogLevel.Info, "Received duplicate IAmADie messages, discarding");
        }
    }

    public void Dispose()
    {
        _ble.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _ble.DisposeAsync();
    }

    internal static PixelsDie Create(BlePeripheral ble)
    {
        var data = ble.GetManufacturerData();
        if (data.Length != 1)
            throw new ArgumentException("No manufacturer data found on peripheral");
        if (data[0].Length != 5)
            throw new ArgumentException($"Incorrect manufacturer data found on peripheral (length {data[0].Length})");
        var die = new PixelsDie(ble);
        var m = data[0];
        die.LedCount = m[0];
        var other = m[1];
        die.RollState = (RollState)m[2];
        die.CurrentFace = m[3];
        var battery = m[4];
        die.BatteryLevel = battery & 0x7F;
        die.BatteryState = battery & 0x80;
        return die;
    }
}