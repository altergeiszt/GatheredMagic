using System;
using System.Collections.Generic;
using HiddenGemShared.Entities;
using HiddenGemBLL.Models;
using HiddenGemBLL.Interfaces;

namespace HiddenGemBLL.Services
{
    /// <summary>
    /// Core business logic service responsible for identifying "Hidden Gems" by running
    /// a multi-pass statistical pipeline. It validates synergy by measuring magngitude,
    /// stability, and statistical significance.
    /// </summary>
    public class SynergyEngine : ISynergyEngine
    {
        private readonly ISynergyFlagService _flagService;
        private readonly IMathService _mathService;

        // A Bayesian weight determines how many "real world" decks it takes to overcome the prior belief.
        private const double BayesianWeight = 100.0;

        public SynergyEngine(ISynergyFlagService flagService, IMathService mathService)
        {
            _flagService = flagService;
            _mathService = mathService;
        }

        public SynergyRelation? ProcessRelationship(Card commander, Card card, DeckStats deckstats)
        {
            // Pass 1: Bayesian Stability (Dynamic Informed Priors)
            // Instead of guessing 5%, we pull the score towards the cards global average.
            // This prevents "niche" cards with 1 decklist from having a 100% synergy score.
            double dynamicAlpha = deckstats.GlobalCardProbability * BayesianWeight;
            double dynamicBeta = BayesianWeight;

            double pSmoothed = (deckstats.SharedCount + dynamicAlpha) / (deckstats.CommanderTotal + dynamicBeta);

            // Pass 2 NPMI Magnitude
            // Measure Strenght of association relative to global probility using smoothed values.
            double npmi = _mathService.CalculateNPMI(pSmoothed, deckstats.GlobalCardProbability);

            // Pass 3 Hypergeometric Validation
            // Prove the association is statistically signifcant (p < 0.05)
            // This is the "confidence filter" that ignores coincedental overlaps.
            double pValue = _mathService.CalculatePValue(
                deckstats.TotalUniveseCount,
                deckstats.GlobalCardCount,
                deckstats.CommanderTotal,
                deckstats.SharedCount
            );

            if (pValue < 0.05 && npmi > 0.3)
            {
                var relation = new SynergyRelation
                {
                    In = commander.Id,
                    Out = card.Id,
                    SynergyScore = npmi,
                    SmoothedRate = pSmoothed,
                    PValue = pValue
                };

                relation.Flags = _flagService.DetectFlags(commander, card);

                return relation;
            }

            return null;
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
        private double CalculateHypergeometricPValue(int k, int n, int KPopulation, int NPopulation)
        {
            // If the card is in 0 decks with the commander, the probability of a fluke is 100%
            if (k == 0) return 1.0;

            // Calculate the probability of seeing AT LEAST 'k' successes.
            // Survivability function: P(X >= k)
            double survivalProbabilityMeasure = 0;

            for (int i = k; i <= Math.Min(n, KPopulation); i++)
            {
                survivalProbabilityMeasure += Math.Exp(LogBinomialCoefficient(KPopulation,i) +
                                                LogBinomialCoefficient(NPopulation-KPopulation, n-i) -
                                                LogBinomialCoefficient(NPopulation,n));
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
    }
}