using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class RolledImportCondition : ImportCondition
{
    [JsonPropertyName("faces")]
    public ImmutableList<int> Faces { get; set; }
}