using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HiddenGemShared.Entities;
using HiddenGemShared.Models;

namespace HiddenGemBLL.Services
{
    /// <summary>
    /// Service responsible for identifying mechanical synergy using the MTGJSON Keyword Data Model.
    /// </summary>
    public class SynergyFlagService : ISynergyFlagService
    {
        /// <summary>
        /// Analyzes the relationship between a Commander and a card by mapping their keywords
        /// to functional categories defined by MTGJSON
        /// </summary>
        public List<SynergyFlag> DetectFlags (Card commander, Card card)
        {
            var discoveredFlags = new List<SynergFlag>();
            
            //1. Structural Comparison (Subtypes and Keywords)
            // Handles "Tribal" and direct keyword matching
            DetectStructuralSynergy(commander, card, discoveredFlags);

            //2. Functional Category Analyses
            // Analyzes commonality withih specific MTGJSON keyword categories.
            DetectCategorySynergy(commander, card, discoveredFlags);

            return discoveredFlags;
        }

        private void DetectStructuralSynergy(Card commander, Card card, List<SynergyFlag> flag)
        {
            // Subtype overlap
            var sharedSubtypes = commander.Subtypes.Intersect(card.Subtypes).ToList();
            if (sharedSubtypes.Any())
            {
                flags.Add(new SynergyFlag("Archetype", "Shared Subtype", $"Cards share creature types: {string.Join(", ", sharedSubtypes)}."));
            }

            // Direct Keyword Match
            var sharedKeywords = commander.Keywords.Intersect(card.Keywords).ToList();
            if (sharedKeywords.Any())
            {
                flags.Add(new SynergyFlag("Archetype", "Keyword Alignment", $"Cards share core mechanics: {string.Join(", ", sharedKeywords)}."));
            }     
        }

        private void DetectCategorySynergy(Card commander, Card card, List<SynergyFlag> flag)
        {
            //This categorizes and explain why there's a synergy between a Commander and a flag.

            //Pass A Keyword Actions 
            var sharedActions = commander.KeywordActions.Intersect(card.KeywordActions).ToList();
            if (sharedActions.Count >= 1)
            {
                flags.Add(new SynergyFlag("Fuel", "Action Synergy", $"Both cards utilize shared actions: {string.Join(", ", sharedActions)}."));
            }

            // Pass B Ability words
            var sharedAbilityWords = commander.AbilityWords.Intersect(card.AbilityWords).ToList();
            if (sharedAbilityWords.Any())
            {
                flags.Add(new SynergyFlag("Archetype", "Strategy Ancor", $"Both cards utilize a shared ability: {string.Join(", ", sharedAbilityWords)}."));
            }

            // Pass C Resource Engine
            if (Commander.RulesText.Contains("Add") && card.RulesText.Contains("Sacrifice"))
            {
                flags.Add(new SynergyFlag("Fuel", "Resource Engine", "Possible mana generation/sacrifice loop detected."));
            }
        }
    }
}