using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VaettirNet.PixelsDice.Net;

public sealed class DieSender : IDisposable, IAsyncDisposable
{
    private JsonSerializerOptions _serializer = BuildSerializerOptions();
    
    private readonly List<PixelsDie> _monitoredDice = [];
    private List<(string url, IImmutableDictionary<string, string> headers)> _urls = [];
    private readonly HttpClient _client = new();
    
    private readonly Dictionary<string, string> _lastError = [];
    public event Action<string, bool, string> ResponseReceived;
    
    private readonly Task _sender;
    private readonly Channel<RollMessage> _channel;

    public DieSender()
    {
        _channel = Channel.CreateUnbounded<RollMessage>();
        _sender = SendQueue(_channel.Reader);
    }

    private async Task SendQueue(ChannelReader<RollMessage> reader)
    {
        await foreach (var item in reader.ReadAllAsync())
        {
            await SendRoll(item);
        }
    }

    public void AddDie(PixelsDie die)
    {
        _monitoredDice.Add(die);
        die.RollStateChanged += RollStateChanged;
    }

    public void RemoveDie(PixelsDie die)
    {
        _monitoredDice.Remove(die);
        die.RollStateChanged -= RollStateChanged;
    }

    public void AddUrl(string url, IImmutableDictionary<string, string> headers = default)
    {
        _urls.Add((url, headers));
    }

    public void UpdateUrl(string url, IImmutableDictionary<string, string> headers)
    {
        _urls.RemoveAll(u => u.url == url);
        _urls.Add((url, headers));
    }

    public void RemoveUrl(string url)
    {
        _urls.RemoveAll(u => u.url == url);
        lock (_lastError)
        {
            _lastError.Remove(url);
        }
    }

    private void RollStateChanged(PixelsDie source, RollState state, int value, int index)
    {
        if (state != RollState.OnFace)
            return;

        _channel.Writer.TryWrite(new RollMessage
        {
            Die = new RollDie()
            {
                Color = source.Colorway,
                Type = source.Type,
                DieId = source.PixelId,
                LedCount = source.LedCount,
            },
            Face = index + 1,
            Value = value,
        });
    }

    private async Task SendTestRoll()
    {
        await SendRoll(new RollMessage
        {
            Die = new()
            {
                Color = Colorway.OnyxBlack,
                Type = DieType.D20,
                DieId = 1234,
                LedCount = 20,
            },
            Face = 12,
            Value = 12,
        });
    }

    public string GetLastError(string url)
    {
        lock (_lastError)
        {
            return _lastError.GetValueOrDefault(url);
        }
    }

    private async Task SendRoll(RollMessage msg)
    {
        string serialized = JsonSerializer.Serialize(msg, _serializer);
        // Copy these in case they change
        var copy = _urls.ToList();
        string[] responses = await Task.WhenAll(copy.Select(u => SendSingle(u.url, u.headers, serialized)));
        foreach (var (url, response) in copy.Zip(responses))
        {
            if (response == null)
            {
                lock (_lastError)
                {
                    _lastError.Remove(url.url);
                }

                ResponseReceived?.Invoke(url.url, true, null);
            }
            else
            {
                lock (_lastError)
                {
                    _lastError[url.url] = response;
                }

                ResponseReceived?.Invoke(url.url, false, response);
            }
        }
    }

    private async Task<string> SendSingle(string url, IImmutableDictionary<string,string> headers, string body)
    {
        using HttpRequestMessage msg = new(HttpMethod.Post, url);
        if (headers != null)
        {
            foreach (KeyValuePair<string, string> h in headers)
            {
                msg.Headers.Add(h.Key, h.Value);
            }
        }

        msg.Content = new StringContent(body, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _client.SendAsync(msg);
            if (response.IsSuccessStatusCode)
                return null;

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var buffer = new char[1000];
            await reader.ReadAsync(buffer, 0, buffer.Length);

            return $"Server response {response.StatusCode}: {new string(buffer)}";
        }
        catch (Exception e)
        {
            return $"Exception: {e.Message}";
        }
    }

    public static JsonSerializerOptions BuildSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
        };
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var die in _monitoredDice)
        {
            RemoveDie(die);
        }

        _channel.Writer.Complete();
        try
        {
            await _sender;
        }
        catch (Exception)
        {
            // Don't care
        }

        if (_client != null) await CastAndDispose(_client);
        if (_sender != null) await CastAndDispose(_sender);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}

public class RollDie
{
    public uint DieId { get; init; }
    public DieType Type { get; init; }
    public Colorway Color { get; init; }
    public int LedCount { get; init; }
}

public class RollMessage
{
    public RollDie Die { get; init; }
    public int Face { get; init; }
    public int Value { get; init; }
}