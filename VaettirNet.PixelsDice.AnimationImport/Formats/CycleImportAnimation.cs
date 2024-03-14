using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class CycleImportAnimation : ImportAnimation
{
    [JsonPropertyName("track")]
    public ImportRgbTrack Track { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;

    [JsonPropertyName("fade")]
    public double Fade { get; set; }

    [JsonPropertyName("intensity")]
    public double Intensity { get; set; } = 1;

    [JsonPropertyName("cycles")]
    public int Cycles { get; set; } = 1;

    [JsonPropertyName("faces")]
    public ImmutableList<int> Faces { get; set; }
}