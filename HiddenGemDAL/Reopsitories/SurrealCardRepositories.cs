using System.ComponentModel.DataAnnotations;
using HiddenGemShared.Entities;
using HiddenGemShared.Interfaces;
using SurrealDb.Net;

namespace HiddenGemDAL.Repositories
{
    public class SurrealCardRepository : ICardRepository
    {
        private readonly ISurrealDbCliet _cliet;

        public SurrealCardRepository (ISurrealDbClient client)
        {
            _client = client;
        }

        public async Task<Card> GetCardByIdAsync(string id)
        {
            // SurrealDB will handle IDs like "card:sol_ring"
            return await _client.Select<Card>(id);
        }

        public async Task UpsertCardAsync(Card card)
        {
            // UPSERT (Insert and Update operation) ensures
            // we don't have duplicate cards during the ETL proccess
            await _client.Upsert(card);
        }

        /// <summary>
        /// Persists a calculated synergy relationship between a Commander and a card in the graph Database.
        /// It uses the SurrealQL 'RELATE' command to create a directional edge (synergizes_with),
        /// that stores the multi-pass result of the Master Pipeline
        /// </summary>
        /// <param name="commanderId">Unique SurrealDB record ID for the Commander node.</param>
        /// <param name="cardId">Unique SurrealDB record ID for the suggested card (Hidden Gems)</param>
        /// <param name="synergyScore">The output of an Normalized Pointwise Mutual Information function.</param>
        /// <param name="pValue">The statistical significance of a pairing between a Commander and a Hidden Gem calculated through Hypergeometric Distribution.</param>
        /// <param name="smoothedRate"> Bayesian-smoothed inclusion rate that ensures data stablitiu.</param>
        /// <returns>A Task representing the asyncrhonous operation of writing to the graphing engine.</returns>
        public async Task CreateSynergyAsync(string commanderId, string cardId,
            double synergyScore, double pValue, double smoothedRate)
        {
            // Graph Edge Creation
            string query = @"
                RELATE $commander->synergizes_with->$gem
                SET
                    synergy_score = $score,
                    p_value = $p,
                    smoothed_rate = $smoothed,
                    last_calculated = time::now();";

            var parameters = new Dictionary<string, object>
            {
                {"commander", commanderId},
                {"gem", cardId},
                {"score", synergyScore},
                {"p", pValue},
                {"smoothed", smoothedRate}
            };

            await _client.Query(query, parameters);
        }

        /// <summary>
        /// Traverses the synergy graph to find "Hidden Gems" for a specific Commander.
        /// It targets the 'synergizes_with' relationship edges rather than running SQL JOIN operations.
        /// </summary>
        /// <param name="commanderId"> A unique SurrealDB record ID for a specific commander.</param>
        /// <param name="minScore"> The minimum threshold that filters out generic staples.</param>
        /// <returns> Returns a collection of relationships containing scores and p-Values orderded by higher impact.</returns>
        public async Task<IEnumerable<SynergyRelation>> GetSynergiesByCommanderAsync (string commanderId, double minScore = 0.0)
        {
            /* Step by step breakdown of the query.
                    1. Select everything from table "synergizes_with" (edges).
                    2. Filter records that matches the specific commanderId.
                    3. Filters for a synergy score greater than the minScore
                    4. Sorts the result in descending order (highest synergy first)
            */
            string query = "SELECT * FROM synergizes_with WHERE in = $commander and synergy_score > $min ORDER BY synergy_score DESC;";  
            
            var result = await _client.Query(query, new {commander = commanderId, min = minScore});
            return result.GetValue<IEnumerable<SynergyRelation>>(0);
        }

        public async Task UpdateGlobalCountAsync(Dictionary<string, int> cardCounts)
        {
            // Batch update for the BlL's memoization logic.
            foreach (var (cardId, count) in cardCounts)
            {
                await _client.Query("UPDATE $id SET global_count += $count;", new {id = cardId, count = count});
            }
        }
    }
}