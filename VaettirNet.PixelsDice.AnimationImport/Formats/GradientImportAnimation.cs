using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class GradientImportAnimation : ImportAnimation
{
    [JsonPropertyName("faces")]
    public ImmutableList<int> Faces { get; set; }
    [JsonPropertyName("track")]
    public ImportRgbTrack Track { get; set; }
}