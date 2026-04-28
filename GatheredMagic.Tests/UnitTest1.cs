namespace GatheredMagic.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using HiddenGemBLL.Services;
using HiddenGemShared.Entities;
using Xunit;

public class SynergyFlagServiceTests
{
    private SynergyFlagService CreateService() => new SynergyFlagService();

    [Fact]
    public void DetectFlags_ReturnsEmpty_WhenNoSharedFlagsTrigger()
    {
        var svc = CreateService();
        var a = new Card();
        var b = new Card();

        var flags = svc.DetectFlags(a, b);

        Assert.Empty(flags);
    }

    [Fact]
    public void DetectFlags_IncludesColorIdentity_WhenSharedColors()
    {
        var svc = CreateService();
        var commander = new Card { ColorIdentity = new List<string> { "W", "U", "B", "R", "G" } };
        var land = new Card { ColorIdentity = new List<string> { "W" } };

        var flags = svc.DetectFlags(commander, land);

        Assert.Contains(flags, f => f.Label == "Color Identity");
        Assert.Contains(flags, f => f.CreatureType == "Color");
    }

    [Fact]
    public void DetectFlags_IsCaseInsensitiveAndDeduplicates_Archetype()
    {
        var svc = CreateService();
        var commander = new Card { CreatureSubtypes = new List<string> { "GIANT", "giant", "ELF" } };
        var deckhand = new Card { CreatureSubtypes = new List<string> { "Giant", "Elf" } };

        var flags = svc.DetectFlags(commander, deckhand);

        Assert.Contains(flags, f => f.CreatureType == "Archetype");
        Assert.Equal(1, flags.Count(f => f.CreatureType == "Archetype"));
    }

    [Fact]
    public void DetectFlags_ThrowsOnNullArguments()
    {
        var svc = CreateService();
        Assert.Throws<NullReferenceException>(() => svc.DetectFlags(null!, new Card()));
        Assert.Throws<NullReferenceException>(() => svc.DetectFlags(new Card(), null!));
    }

    [Fact]
    public void DetectFlags_ReturnsNone_WhenNoFlagsTrigger()
    {
        var svc = CreateService();
        var commander = new Card
        {
            CreatureSubtypes = new List<string> { "Eldrazi" },
            KeywordAbilities = new List<string> { "Trample" },
            KeywordActions = new List<string> { "Sacrifice" },
            ColorIdentity = new List<string> { "U" }
        };

        var deckhand = new Card
        {
            CreatureSubtypes = new List<string> { "Cat" },
            KeywordAbilities = new List<string> { "Fear" },
            KeywordActions = new List<string> { "Adapt" },
            AbilityWords = new List<string> { "Alliance" },
            ColorIdentity = new List<string> { "W" }
        };

        var flags = svc.DetectFlags(commander, deckhand);

        Assert.Empty(flags);
    }

    [Fact]
    public void DetectFlags_CreatesFuelFlag_WhenActionsOverlap()
    {
        var svc = CreateService();

        var commander = new Card
        {
            CreatureSubtypes = new List<string> { "Eldrazi" },
            KeywordAbilities = new List<string> { "Devoid" },
            KeywordActions = new List<string> { "Add" },
            ColorIdentity = new List<string> { "W" },
        };

        var deckhand = new Card
        {
            CreatureSubtypes = new List<string> { "Elemental" },
            KeywordAbilities = new List<string> { "Myriad" },
            KeywordActions = new List<string> { "Add" },
            ColorIdentity = new List<string> { "B" },
        };

        var flags = svc.DetectFlags(commander, deckhand);

        Assert.Contains(flags, f => f.CreatureType == "Fuel");
        Assert.Contains(flags, f => f.Label == "Action Synergy");
    }

    [Fact]
    public void DetectFlags_MultipleFlags_WhenSeveralOverlap()
    {
        var svc = CreateService();

        var commander = new Card
        {
            CreatureSubtypes = new List<string> { "Goblin", "Warlock" },
            KeywordActions = new List<string> { "Blight" },
            ColorIdentity = new List<string> { "B", "G", "R" },
        };

        var deckhand = new Card
        {
            CreatureSubtypes = new List<string> { "Warlock" },
            KeywordActions = new List<string> { "Blight" },
            ColorIdentity = new List<string> { "B", "G" }
        };

        var flags = svc.DetectFlags(commander, deckhand);

        Assert.True(flags.Count >= 2);
        Assert.Contains(flags, f => f.CreatureType == "Archetype");
        Assert.Contains(flags, f => f.CreatureType == "Fuel");
        Assert.Contains(flags, f => f.CreatureType == "Color");
    }
}