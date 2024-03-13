using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class SimpleImportAnimation : ImportAnimation
{
    [JsonPropertyName("faces")]
    public ImmutableList<int> Faces { get; set; }
    [JsonPropertyName("color")]
    public string Color { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("fade")]
    public double Fade { get; set; }
}