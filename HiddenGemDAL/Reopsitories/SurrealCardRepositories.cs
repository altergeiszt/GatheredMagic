using HiddenGemShared.Entities;
using HiddenGemShared.Interfaces;
using SurrealDb.Net;

namespace HiddenGemDAL.Repositories
{
    public class SurrealCardRepository : ICardRepository
    {
        private readonly ISurrealDbClient _client;

        public SurrealCardRepository (ISurrealDbClient client)
        {
            _client = client;
        }
    public async Task<Card> GetCardByIdAsync(string id)
    {
        // Accept either "sol_ring" or "card:sol_ring" and normalize to record id.
        string normalizedId = id.StartsWith("card:", StringComparison.OrdinalIgnoreCase)
            ? id["card:".Length..]
            : id;

        var card = await _client.Select<Card>(("card", normalizedId));

        if (card is null)
        {
            throw new KeyNotFoundException($"Card not found: {id}");
        }

        return card;
    }

    public async Task UpsertCardAsync(Card card)
    {
        // SurrealDb.Net 0.9.0 typed Upsert requires IRecord; use query-based UPSERT for now.
        string normalizedId = card.Id.StartsWith("card:", StringComparison.OrdinalIgnoreCase)
            ? card.Id["card:".Length..]
            : card.Id;

        await _client.Query($"UPSERT card:{normalizedId} CONTENT {card};");
    }

    public async Task CreateSynergyAsync(
        string commanderId,
        string cardId,
        double synergyScore,
        double pValue,
        double smoothedRate)
    {
        await _client.Query($"""
            RELATE {commanderId}->synergizes_with->{cardId}
            SET
                synergy_score = {synergyScore},
                p_value = {pValue},
                smoothed_rate = {smoothedRate},
                last_calculated = time::now();
            """);
    }

        /// <summary>
        /// Traverses the synergy graph to find "Hidden Gems" for a specific Commander.
        /// It targets the 'synergizes_with' relationship edges rather than running SQL JOIN operations.
        /// </summary>
        /// <param name="commanderId"> A unique SurrealDB record ID for a specific commander.</param>
        /// <param name="minScore"> The minimum threshold that filters out generic staples.</param>
        /// <returns> Returns a collection of relationships containing scores and p-Values orderded by higher impact.</returns>
    public async Task<IEnumerable<SynergyRelation>> GetSynergiesByCommanderAsync(
        string commanderId,
        double minScore = 0.0)
    {
        var result = await _client.Query($"""
            SELECT * FROM synergizes_with
            WHERE in = {commanderId}
            AND synergy_score > {minScore}
            ORDER BY synergy_score DESC;
            """);

        return result.GetValue<IEnumerable<SynergyRelation>>(0) ?? Enumerable.Empty<SynergyRelation>();
    }

    public async Task UpdateGlobalCountsAsync(Dictionary<string, int> cardCounts)
    {
        foreach (var (cardId, count) in cardCounts)
        {
            await _client.Query($"UPDATE {cardId} SET global_count += {count};");
        }
    }
    }
}