namespace HiddenGemShared.Entities;

public class Card
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    public List<string> CreatureType {get; set;} = new();
    public List<string> Subtypes {get; set;} = new();
    public List<string> ColorIdentity {get; set;} = new();
    public int ManaValue {get; set;}
    public List<string> ManaCost {get;set;} = new();
    public List<string> AbilityWords {get; set;} = new(); 
    public List<string> KeywordAbilities {get; set;} = new(); //[Haste, Trample, Annihalator]
    public List<string> KeywordActions {get; set;} = new();
    public string RulesText {get;set;} = string.Empty;

}