using System.Linq;
using System.Text.Json;
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

        public async Task UpsertCardAsync(Card card)
        {
            await _client.Upsert($"card:{card.Id}",card);
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

        public async Task<Card?> GetCardByNameAsync(string cardName)
        {
            // Sanitize or format the card name to match your ID structure
            var sanitizedId = cardName.ToLower().Replace(" ", "_").Replace(",", "");
            var result = await _client.Select<Card>($"card:{sanitizedId}");
            
            return result?.FirstOrDefault();
        }

        public async Task CreateSynergyRelationsAsync(SynergyRelation relation)
        {
            var relationJson = JsonSerializer.Serialize(relation);

            await _client.Query($"""
                RELATE card:{relation.CommanderId}->synergizes_with->card:{relation.DeckhandId}
                CONTENT {relationJson};
                """);
        }
        public async Task<List<SynergyRelation>> GetSynergiesForCommanderAsync(string commanderId)
        {
            // Graph Traversal: "Find all outgoing synergy edges starting from this specific Commander"
            var query = $"SELECT * FROM synergizes_with WHERE in = card:{commanderId};";
            
            var response = await _client.Query($"{query}");
            return response.GetValue<List<SynergyRelation>>(0) ?? new List<SynergyRelation>();
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

        public async Task UpdateGlobalCountsAsync(Dictionary<string, int> cardCounts)
        {
            foreach (var (cardId, count) in cardCounts)
            {
                await _client.Query($"UPDATE {cardId} SET global_count += {count};");
            }
        }
    }
}