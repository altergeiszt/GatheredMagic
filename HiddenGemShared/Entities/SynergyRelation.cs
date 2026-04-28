using HiddenGemShared.Models;

namespace HiddenGemShared.Entities;

public class SynergyRelation
{
    // SurrealDB Edge Identifiers
    public string Id {get;set;} = string.Empty;
    public string CommanderId {get; set;} = string.Empty;
    public string DeckhandId {get; set;} = string.Empty;

    // Statical scores
    public double SynergyScore {get; set;}
    public double SmoothedRate {get; set;}
    public double PValue {get; set;}
    
    // Mechanical Tagging
    public List<SynergyFlag> Flags {get; set;} = new();

    //Metadata from ETL process
    public DateTime CalculatedAt {get; set;} = DateTime.UtcNow;
    public string Source {get; set;} = "Pipeline_v1";

}
