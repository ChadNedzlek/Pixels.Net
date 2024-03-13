using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CycleImportAnimation), typeDiscriminator: "cycle")]
[JsonDerivedType(typeof(GradientImportAnimation), typeDiscriminator: "gradient")]
[JsonDerivedType(typeof(GradientPatternImportAnimation), typeDiscriminator: "gradientPattern")]
[JsonDerivedType(typeof(KeyFramedImportAnimation), typeDiscriminator: "keyFramed")]
[JsonDerivedType(typeof(NoiseImportAnimation), typeDiscriminator: "noise")]
[JsonDerivedType(typeof(NormalsImportAnimation), typeDiscriminator: "normals")]
[JsonDerivedType(typeof(RainbowImportAnimation), typeDiscriminator: "rainbow")]
[JsonDerivedType(typeof(SimpleImportAnimation), typeDiscriminator: "simple")]
[JsonDerivedType(typeof(WormImportAnimation), typeDiscriminator: "worm")]
public abstract class ImportAnimation
{
    [JsonPropertyName("durationMs")]
    public int DurationInMs { get; set; }
}