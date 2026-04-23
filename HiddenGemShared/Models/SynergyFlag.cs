namespace HiddenGemShared.Models;

public class SynergyFlag
{
    public string CreatureType { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public SynergyFlag(string creaturetype, string label, string description)
    {
        CreatureType = creaturetype;
        Label = label;
        Description = description;
    }
}