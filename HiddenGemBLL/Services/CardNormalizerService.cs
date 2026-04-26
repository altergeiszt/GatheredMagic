using System.Text.Json;
using System.Text.RegularExpressions;
using HiddenGemShared.Models;
using HiddenGemShared.Entities;

namespace HiddenGemBLL.Services;

public interface ICardNormalizerService
{
    List<string> ParseCreatureSubtypes(string rawTypeLine);
    void NormalizeRulesText(Card card);
}

public class CardNormalizerService : ICardNormalizerService
{
    private readonly HashSet<string> _abilityWords;
    private readonly HashSet<string> _keywordAbilities;
    private readonly HashSet<string> _keywordActions;

    public CardNormalizerService(string jsonFilePath)
    {
        if (string.IsNullOrWhiteSpace(jsonFilePath))
        {
            throw new ArgumentException("Keywords.json path is required.", nameof(jsonFilePath));
        }

        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException("Keywords.json file was not found.", jsonFilePath);
        }

        // Read and deserialize Keywords.json
        var jsonString = File.ReadAllText(jsonFilePath);
        var parsedData = JsonSerializer.Deserialize<KeywordRoot>(jsonString)
            ?? throw new InvalidDataException("Keywords.json could not be deserialized.");

        if (parsedData.Data is null)
        {
            throw new InvalidDataException("Keywords.json is missing the 'Data' section.");
        }

        // Load into case-insensitive Hashsets
        var comparer = StringComparer.OrdinalIgnoreCase;
        _abilityWords = new HashSet<string>(parsedData.Data.AbilityWords ?? [], comparer);
        _keywordAbilities = new HashSet<string>(parsedData.Data.KeywordAbilities ?? [], comparer);
        _keywordActions = new HashSet<string>(parsedData.Data.KeywordActions ?? [], comparer);
    }

    public List<string> ParseCreatureSubtypes(string rawTypeLine)
    {
        if (string.IsNullOrWhiteSpace(rawTypeLine))
        {
            return new List<string>();
        }

        // In Magic type lines, supertypes/types and subtypes are separated by a dash token.
        // Match separators with surrounding whitespace to avoid splitting hyphenated words.
        Match typeLineMatch = Regex.Match(rawTypeLine, @"^\s*.+?\s+[—-]\s+(?<subtypes>.+?)\s*$");
        if (!typeLineMatch.Success)
        {
            return new List<string>();
        }

        string subTypeString = typeLineMatch.Groups["subtypes"].Value;

        var subtypes = subTypeString.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim())
            .ToList();
        
        return subtypes;
    }
    public void NormalizeRulesText(Card card)
        {
            if (string.IsNullOrWhiteSpace(card.RulesText)) return;

            // Clean up existing data to avoid duplicates if reprocessed.
            card.AbilityWords.Clear();
            card.KeywordAbilities.Clear();
            card.KeywordActions.Clear();

            ExtractTokens(card.RulesText, _abilityWords, card.AbilityWords);
            ExtractTokens(card.RulesText, _keywordAbilities, card.KeywordAbilities);
            ExtractTokens(card.RulesText, _keywordActions, card.KeywordActions);
        }

    private void ExtractTokens(string rulesText, HashSet<string> canonicalList, List<string> targetCardsList)
        {
            foreach (var keyword in canonicalList)
            {
                // We should use RegEx here with word boundaries (\b) to prevent partial matches
                string pattern = $@"\b{Regex.Escape(keyword)}\b";
                if (Regex.IsMatch(rulesText, pattern, RegexOptions.IgnoreCase))
                {
                    targetCardsList.Add(keyword);
                }
            }
        }
}