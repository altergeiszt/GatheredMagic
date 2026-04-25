namespace HiddenGemShared.Entities;

public class Card
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    private string _creatureType = string.Empty;
    public int Mana {get; set;}
    public List<string> Keywords {get; set;} = new(); //[Haste, Trample, Annihalator]
    public List<string> Abilities {get; set;} = new(); //[Tap, Draw, Sacrifice]
    public string Text {get; set;} = string.Empty; // Full card Text
    public List<string> KeywordActions {get; set;} = new();
    public List<string> AbilityWords {get; set;} = new();
    public string RulesText {get;set;} = string.Empty;

    // The next two fields has been refactored so that we can test the system to check if
    // the system can detect variations in raw strings.
    // We do this by parsing and normalizing CreatureType as a token and then merge it to the
    // Subtype field, then our "SynergyFlagService.DetectStructuralSynergy" would be able to
    // trigger an "Archetype Flag" between the Commander and Gem card.
    // Please refer to the DesignConsideration documents as to why I find this neccessary.
    public string CreatureType
    {
        get => _creatureType;
        set
        {
            _creatureType = value;
            NormalizeAndMergeSubtypes(value);
        }
    } 
    
    public List<string> Subtypes {get; set;} = new(); // [Eldrazi Scion]
    private void NormalizeAndMergeSubtypes(string typeLine)
    {
        if (string.IsNullOrWhiteSpace(typeLine)) return;

        // We do this by stripping the dashes (-) and whitespaces found in the card details.
        // Example Legendary Creature - Eldrazi Titan would be mapped to a list like
        // {"Legendary", "Creature", "Eldrazi", "Titan"}.
        var parts = typeLine.Split('-');
        
        // We use a ternary operator to check if the array has more than one element.
        string subtypePart = parts.Length > 1 ? parts[1] : parts[0];

        // We then tokenize by splitting on whitespaces and removing empty entries by doing:
        // We use the LINQ .Select() statement and pass the anonymous (lambda) function to clean the tokens
        // .Select(t => t.Trim()) 
        var tokens = subtypePart.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim()) 
                                .ToList();
        
        // We then merge the token with existing Subtypes, and ensure there are no duplicates.
        foreach (var token in tokens)
        {
            if (!Subtypes.Contains(token, StringComparer.OrdinalIgnoreCase))
            {
                Subtypes.Add(token);
            }
        }
    }
}

