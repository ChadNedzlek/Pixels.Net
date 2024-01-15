using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pixels.Net.Ble;
using Pixels.Net.Messages;

namespace Pixels.Net;

public sealed class PixelsDie : IDisposable
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

    public event Action<PixelsDie, RollState, int> RollStateChanged;

    private PixelsDie(BlePeripheral ble)
    {
        _ble = ble;
    }

    public async Task ConnectAsync()
    {
        await _ble.ConnectAsync(DataReceived);
        _ble.SendMessage(new WhoAmIMessage());
        IAmADieMessage id = await _idReceived.Task;
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
    
    public string GetPersistentIdentifier()
    {
        return _ble.GetPersistentId();
    }

    public void Blink(int count, TimeSpan duration, Color color, byte faceMask, double fade, bool loop)
    {
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

    private readonly TaskCompletionSource<IAmADieMessage> _idReceived = new();

    private void DataReceived(ReadOnlySpan<byte> msg)
    {
        if (msg.Length == 0)
            return;

        switch (msg[0])
        {
            case 2:
                HandleIAmADie(MemoryMarshal.Read<IAmADieMessage>(msg));
                break;
            case 3:
                HandleDieRoll(MemoryMarshal.Read<RollStateMessage>(msg));
                break;
            case 34:
                HandleBatteryMessage(MemoryMarshal.Read<BatteryLevelMessage>(msg));
                break;
            default:
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
        RollStateChanged?.Invoke(this, msg.RollState, msg.CurrentFace);
    }

    private void HandleIAmADie(IAmADieMessage msg)
    {
        _idReceived.SetResult(msg);
    }

    public void Dispose()
    {
        _ble.Dispose();
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