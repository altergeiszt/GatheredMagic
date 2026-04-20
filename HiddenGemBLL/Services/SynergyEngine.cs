using HiddenGemShared.Interfaces;
using HiddenGemShared.Entities;
using System.Math;
using System.ComponentModel.DataAnnotations;

namespace HiddenGemBLL.Services
{
    /// <summary>
    /// Core business logic service responsible for identifying "Hidden Gems" by running
    /// a multi-pass statistical pipeline. It validates synergy by measuring magngitude,
    /// stability, and statistical significance.
    /// </summary>
    public class SynergyEngine
    {
        private readonly ICardRepository _repository;

        // Bayesian constants that acts as baseline decks to stablilize results.
        private const double Alpha = 2.0;
        private const double Beta = 10.0;

        public SynergyEngine(ICardRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Executes the Master Pipeline: Bayesian Refinement -> NMPI Calculation -> Hypergeometric filtering.
        /// It ensures non-staple synergies are persisted with high statistical confidence.
        /// </summary>
        /// <param name="commanderId"> Unique ID of the commander card being analyzed.</param>
        /// <param name="totalDecksInMeta">Total number of decks in the rolling 24-month window.</param>
        /// <returns></returns>
        public async Task ProccessCommanderSynergiesAsync(string commanderId, int totalDecksInMeta)
        {
            // 1. Fetch raw data from DAL
            var potentialGems = await _repository.GetRawCountsForCommander(commanderId);
            var commanderDeckCount = await _repository.GetDeckCountForCommander(commanderId);

            foreach (var card in potentialGems)
            {
                // Pass 1: Bayesian Refinement (Stability)
                // Prevents "New Card" hype or small sample sizes from inflating the score.
                double smoothedCommanderRate = (card.InclusionInCommanderDeck + Alpha) / (commanderDeckCount + Beta);
                double smoothedGlobalRate = (card.GlobalInclusionCount + Alpha) / (totalDecksInMeta + Beta);

                // Pass 2: NPMI Calculation (Magnitude)
                // Generic staples are penalized by measuring co-occurence against random chance.
                double npmiScore = CalculateNPMI(smoothedCommanderRate, smoothedGlobalRate);

                // Pass 3: Hypergeometric Test (Significance)
                // Discards any pairings where the overlap could be a mathematical fluke.
                double pValue = CalculateHyperGeometricPValue(
                    card.InclusionInCommanderDecks,
                    commanderDeckCount,
                    card.GlobalInclusionCount,
                    totalDecksInMeta);

                // Final Valitadion: Saves pairings if p-Value meets 0.05 threshold of significance.
                if (pValue < 0.05 && npmiScore >0)
                {
                    await _repository.CreateSynergyAsync(commanderId, card.Id, npmiScore, pValue, smoothedCommanderRate);
                }
            }
        }

        /// <summary>
        /// Calculates Normalized Pointwise Information
        /// </summary>
        /// <param name="pC_and_X">Joint probability of having Commander and a gem together in a deck together</param>
        /// <param name="pX">Baseline probability of a gem appearing throughout the format </param>
        /// <returns> 1 perfect co-occurance (card is only seen with this specific Commander)
        ///           0 complete independence (card appears exactly as often as random chance predicts)
        ///          -1 Mutually exclusive (the cards never appear together)
        /// </returns>
        private double CalcluateNMPI (double pC_and_X, double pX)
        {
            if (pC_and_X == 0) return -1;
            double pmi = Math.Log2(pC_and_X/pX);
            return pmi / (-Math.Log2(pC_and_X));
        }

        /// <summary>
        /// Calculates the statistical significance of a card's inclusion using the
        /// Hypergeometric Distribution. It determines the probability 
        /// that the observed overlap is due to random chance.
        /// </summary>
        /// <param name="k">Number of Success within the Sample (Decks containing both the commander and gem</param>
        /// <param name="n">Total number of decks for this specific Commander.</param>
        /// <param name="K">Number of Success within the Population (Decks in the meta containing the Card)</param>
        /// <param name="N">Total number of decks in the meta window</param>
        /// <returns>A pValue between 0 and 1 where Values < 0.05 suggests statistical significance.</returns>
        private double CalculateHypergeometricPValue(int k, int n, int K, int N)
        {
            // If the card is in 0 decks with the commander, the probability of a fluke is 100%
            if (k == 0) return 1.0;

            double logPValue = 0;

            // Calculate the probability of seeing AT LEAST 'k' successes.
            // Survivability function: P(X >= k)
            double survivalProbabilityMeasure = 0;

            for (int i = k; i <= Math.Min(n, K); i++)
            {
                survivalProbabilityMeasure += Math.Exp(LogBinomialCoefficient(K,i) +
                                                LogBinomialCoefficient(N-K, n-i) -
                                                LogBinomialCoefficient(N,n));
            }
            return Math.Clamp(survivalProbabilityMeasure, 0.0, 1.0);
        }

        /// <summary>
        /// Calculates the natural log of a binomial coefficient (nCr)
        /// Uses Log-Gamma to prevent overflow
        /// </summary>
        /// <param name="n"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private double LogBinomialCoefficient(int n, int r)
        {
            if (r < 0 || r > n ) return double.NegativeInfinity;
            return LogFactorial(n) - (LogFactorial(r) + LogFactorial(n-r));
        }

        private double LogFactorial(int n)
        {
            //System.Math.LogGamma
            return Math.LogGamma(n + 1);
        }
    }
}