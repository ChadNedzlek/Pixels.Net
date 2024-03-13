using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class KeyFramedImportAnimation : ImportAnimation
{
    [JsonPropertyName("tracks")]
    public ImmutableList<ImportRgbTrack> Tracks { get; set; }
}