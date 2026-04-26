using System.Text.Json;
using System.Text.RegularExpressions;
using HiddenGemShared.Models;
using HiddenGemShared.Entities;

namespace HiddenGemBLL.Services;

public interface ICardNormalizerService
{
    void NormalizeRulesText(Card card);
}

public class CardNormalizerService : ICardNormalizerService
{
    private readonly HashSet<string> _abilityWords;
    private readonly HashSet<string> _keywordAbilities;
    private readonly HashSet<string> _keywordActions;

    public CardNormalizerService(string jsonFilePath)
    {
        // Read and deserialize Keywords.json
        var jsonString = File.ReadAllText(jsonFilePath);
        var parsedData = JsonSerializer.Deserialize<KeywordRoot>(jsonString);

        // Load into case-insensitive Hashsets
        var comparer = StringComparer.OrdinalIgnoreCase;
        _abilityWords = new HashSet<string>(parsedData.Data.AbilityWords, comparer);
        _keywordAbilities = new HashSet<string>(parsedData.Data.KeywordAbilities, comparer);
        _keywordActions = new HashSet<string>(parsedData.Data.KeywordActions, comparer);
    }

    public List<string> ParseCreatureSubtypes(string rawTypeLine)
    {
        if (string.IsNullOrWhiteSpace(rawTypeLine))
        {
            return new List<string>();
        }

        // In Magic: The Gathering Cards type lines, a card's type and subtype
        // are seperated with an em dash ("—") or a hyphen ("-")
        string[] parts = rawTypeLine.Split(new[] {"—", "-"}, StringSplitOptions.RemoveEmptyEntries);

        // If there are no dashes the card has no subtype
        if (parts.Length < 2)
        {
            return new List<string>();
        }

        string subTypeString = parts[1];

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
                string pattern = $@"/b{Regex.Escape(keyword)}\b";
                if (Regex.IsMatch(rulesText, pattern, RegexOptions.IgnoreCase))
                {
                    targetCardsList.Add(keyword);
                }
            }
        }
}