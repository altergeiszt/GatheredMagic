using System.Text.Json.Serialization;

namespace HiddenGemShared.Models;

public class KeywordRoot
{
    [JsonPropertyName("data")]
    public KeywordData Data {get; set;} = new();
}

public class KeywordData
{
    [JsonPropertyName("abilityWords")]
    public List<string> AbilityWords {get; set;} = new();

    [JsonPropertyName("keywordAbilities")]
    public List<string> KeywordAbilities {get; set;} = new();

    [JsonPropertyName("keywordActions")]
    public List<string> KeywordActions {get; set;} = new();
}