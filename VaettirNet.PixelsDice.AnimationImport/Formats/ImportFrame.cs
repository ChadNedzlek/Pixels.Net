using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class ImportFrame
{
    [JsonPropertyName("intensity")]
    public double Intensity { get; set; }
    [JsonPropertyName("offsetMs")]
    public int OffsetMs { get; set; }
}