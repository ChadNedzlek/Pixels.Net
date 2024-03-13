using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class GradientPatternImportAnimation : ImportAnimation
{
    [JsonPropertyName("tracks")]
    public ImmutableList<ImportTrack> Tracks { get; set; }
    [JsonPropertyName("colorTrack")]
    public ImportRgbTrack ColorTrack { get;set; }
    [JsonPropertyName("overrideWithFace")]
    public bool OverrideWithFace { get; set; }
}