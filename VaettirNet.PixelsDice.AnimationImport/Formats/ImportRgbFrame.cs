using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class ImportRgbFrame
{
    [JsonPropertyName("color")]
    public string Color { get; set; }
    [JsonPropertyName("offsetMs")]
    public int OffsetMs { get; set; }
}