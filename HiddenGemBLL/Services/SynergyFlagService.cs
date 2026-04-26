using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HiddenGemShared.Entities;
using HiddenGemShared.Models;
using HiddenGemBLL.Interfaces;

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
        public List<SynergyFlag> DetectFlags (Card commander, Card deckhand)
        {
            var discoveredFlags = new List<SynergyFlag>();
            
            // Tribal or Type Synergy
            var sharedSubtypes = commander.CreatureSubtypes
                .Intersect(deckhand.CreatureSubtypes, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (sharedSubtypes.Any())
            {
                discoveredFlags.Add(new SynergyFlag("Archetype", "Shared Lineage",
                $"Both cards are: {string.Join(", ", sharedSubtypes)}"));
            }

            //2. Core Mechanic Synergy
            // Analyzes commonality withih specific MTGJSON keyword categories.
            var sharedAbilities = commander.KeywordAbilities
                .Intersect(deckhand.KeywordAbilities, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (sharedAbilities.Any())
            {
                discoveredFlags.Add(new SynergyFlag("Archetype", "Mechanical Alignment",
                $"Both cards feature: {string.Join(", ", sharedAbilities)}"));
            }

            //3. Action or Fuel Synergy
            var sharedActions = commander.KeywordActions
                .Intersect(deckhand.KeywordActions, StringComparer.OrdinalIgnoreCase)
                .ToList();
            
            if (sharedActions.Any())
            {
                discoveredFlags.Add(new SynergyFlag("Fuel", "Action Synergy",
                $"Both cards utilize the action: {string.Join(", ", sharedActions)}"));
            }

            //4. Theme or Strategy Synergy
            var sharedThemes = commander.AbilityWords
                .Intersect(deckhand.AbilityWords, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (sharedThemes.Any())
            {
                discoveredFlags.Add(new SynergyFlag("Multiplier", "Strategy Anchor",
                $"Bot cards trigger off of: {string.Join(", ", sharedThemes)}"));
            }

            return discoveredFlags;
        }
    }
}