using System.Text.Json.Serialization;
using VaettirNet.PixelsDice.Net.Animations;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class NormalsImportAnimation : ImportAnimation
{
    [JsonPropertyName("angle")]
    public ImportRgbTrack Angle { get; set; }

    [JsonPropertyName("axis")]
    public ImportRgbTrack Axis { get; set; }

    [JsonPropertyName("time")]
    public ImportRgbTrack Time { get; set; }

    [JsonPropertyName("fade")]
    public double Fade { get; set; }

    [JsonPropertyName("axisOffset")]
    public double AxisOffset { get; set; }

    [JsonPropertyName("axisScale")]
    public double AxisScale { get; set; } = 1;

    [JsonPropertyName("axisScroll")]
    public double AxisScrollSpeed { get; set; }

    [JsonPropertyName("angleScroll")]
    public double AngleScrollSpeed { get; set; }

    [JsonPropertyName("colorVar")]
    public double MainGradientColorVariance { get; set; }

    [JsonPropertyName("colorType")]
    public NormalsColorOverrideType OverrideType { get; set; }
}