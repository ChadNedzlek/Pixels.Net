using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class ImportRgbTrack
{
    [JsonPropertyName("frames")]
    public ImmutableList<ImportRgbFrame> Frames { get; set; }
    [JsonPropertyName("leds")]
    public ImmutableList<int> Leds { get; set; }
}
public class ImportTrack
{
    [JsonPropertyName("frames")]
    public ImmutableList<ImportFrame> Frames { get; set; }
    [JsonPropertyName("leds")]
    public ImmutableList<int> Leds { get; set; }
}