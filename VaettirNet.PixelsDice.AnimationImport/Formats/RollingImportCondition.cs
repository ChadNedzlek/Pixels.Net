using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class RollingImportCondition : ImportCondition
{
    [JsonPropertyName("repeatMs")]
    public int RepeatPeriodInMs { get; set; }
}