using System.ComponentModel.DataAnnotations;
using HiddenGemShared.Entities;

namespace HiddenGemShared.Interfaces
{
    public interface ICardRepository
    {
        // CRUD for Cards (Nodes)
        Task<Card> GetCardByIdAsync(string id);
        Task UpsertCardAsync(Card card);

        // Graph Operations for Synergies (Edges)
        Task CreateSynergyAsync(string commanderId, string cardId, double synergyScore, double pValue, double smoothedRate);
        Task<IEnumerable<SynergyRelation>> GetSynergiesByCommanderAsync(string commanderId, double minScore = 0.0);

        // Batch Operations for the Master Pipeline
        Task UpdateGlobalCountsAsync(Dictionary<string, int> cardCounts);
    }
}