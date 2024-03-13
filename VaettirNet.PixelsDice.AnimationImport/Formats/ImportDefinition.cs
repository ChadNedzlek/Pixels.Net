using System.Text.Json.Serialization;
using VaettirNet.PixelsDice.Net.Animations;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class ImportDefinition
{
    [JsonPropertyName("condition")]
    public ImportCondition Condition { get; set; }
    
    [JsonPropertyName("animation")]
    public ImportAnimation Animation { get; set; }

    [JsonPropertyName("face")]
    public int Face { get; set; } = FaceIndex.Current;
}