using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace VaettirNet.PixelsDice.AnimationImport.Formats;

public class ImportProfile
{
    [JsonPropertyName("dice")]
    public ImmutableList<string> Dice { get; set; }
    [JsonPropertyName("definitions")]
    public ImmutableList<ImportDefinition> Definitions { get; set; }
}