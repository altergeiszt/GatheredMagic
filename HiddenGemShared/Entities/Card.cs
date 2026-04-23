namespace HiddenGemShared.Entities;

public class Card
{
    public string Id {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    public string CreatureType {get; set;} = string.Empty; //Creature type
    public int Mana {get; set;}
    public List<string> Keywords {get; set;} = new(); //[Haste, Trample, Annihalator]
    public List<string> Abilities {get; set;} = new(); //[Tap, Draw, Sacrifice]
    public string Text {get; set;} = string.Empty; // Full card Text
    public List<string> Subtypes {get; set;} = new(); // [Eldrazi Scion]
    public List<string> KeywordActions {get; set;} = new();
    public List<string> AbilityWords {get; set;} = new();
    public string RulesText {get;set;} = string.Empty;
}