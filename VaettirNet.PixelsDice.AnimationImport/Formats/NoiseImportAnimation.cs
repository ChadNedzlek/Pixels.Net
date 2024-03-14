using System.Text.Json.Serialization;
using VaettirNet.PixelsDice.Net.Animations;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class NoiseImportAnimation : ImportAnimation
{
    [JsonPropertyName("overall")]
    public ImportRgbTrack OverallGradientTrack { get; set; }
    [JsonPropertyName("individual")]
    public ImportRgbTrack IndividualGradientTrack { get; set; }
    [JsonPropertyName("blinksPerSecond")]
    public double BlinksPerSecond { get; set; }
    [JsonPropertyName("blinksPerSecondVar")]
    public double BlinksPerSecondVariance { get; set; }
    [JsonPropertyName("blinkDurationMs")]
    public int BlinkDurationMs { get; set; }
    [JsonPropertyName("fade")]
    public double Fade { get; set; }
    [JsonPropertyName("colorType")]
    public NoiseColorOverrideType ColorOverrideType { get; set; } = NoiseColorOverrideType.None;
    [JsonPropertyName("colorVar")]
    public double OverallColorVariance { get; set; }
}