using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RolledImportCondition), typeDiscriminator: "rolled")]
[JsonDerivedType(typeof(RollingImportCondition), typeDiscriminator: "rolling")]
public abstract class ImportCondition
{
    
}