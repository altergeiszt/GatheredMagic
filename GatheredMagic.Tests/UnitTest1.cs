namespace GatheredMagic.Tests;

using HiddenGemBLL.Services;
using HiddenGemShared.Entities;

public class SynergyFlagServiceTests
{
    [Fact]
    public void DetectFlags_AddsSharedSubtypeFlag_WhenCommanderAndCardShareSubtype()
    {
        var service = new SynergyFlagService();

        var commander = new Card
        {
            Id = "cmdr-1",
            Name = "Commander",
            Subtypes = new List<string> {"Elf", "Shaman"},
            Keywords = new List<string>(),
            KeywordActions = new List<string>(),
            AbilityWords = new List<string>(),
            RulesText = "Add one mana of any color."
        };

        var card = new Card
        {
            Id = "card-1",
            Name = "Gem card",
            Subtypes = new List<string> {"Elf"},
            Keywords = new List<string>(),
            KeywordActions = new List<string>(),
            AbilityWords = new List<string>(),
            RulesText = "Sacrifice another creature."
        };

        var flags = service.DetectFlags(commander, card);

        Assert.Contains(flags, f => f.Label == "Shared Subtype");
    }

    [Fact]
    public void DetectFlags_ReturnEmpty_WhenNoOverlapAndNoResourcePattern()
    {
        var service = new SynergyFlagService();

        var commander = new Card
        {
            Id = "cmdr-2",
            Name = "Commander",
            Subtypes = new List<string>{"Wizard"},
            Keywords = new List<string>{"Flying"},
            KeywordActions = new List<string> {"Draw"},
            AbilityWords = new List<string> {"Threshold"},
            RulesText = "Scry 1."
        };

        var card = new Card
        {
            Id = "card-2",
            Name = "Gem Card",
            Subtypes = new List<string> {"Zombie"},
            Keywords = new List<string> {"Trample"},
            KeywordActions = new List<string>{"Exile"},
            AbilityWords = new List<string>{"Landfall"},
            RulesText = "Gain 1 life."
        };

        var flags = service.DetectFlags(commander, card);

        Assert.Empty(flags);
    }
    
    [Fact]
    public void DetectFlags_AddsSharedSubtypeFlag_WhenCreatureTypeTokensOverlap()
    {
        var service = new SynergyFlagService();

        var commander = new Card
        {
            Id = "cmdr-3",
            Name = "Commander",
            CreatureType = " - ELDRAZI Horror",
            Keywords = new List<string>(),
            KeywordActions = new List<string>(),
            AbilityWords = new List<string>(),
        };

        var card = new Card
        {
            Id = "card-3",
            Name = "Gem Card",
            CreatureType = "Eldrazi",
            Keywords = new List<string>(),
            KeywordActions = new List<string>(),
            AbilityWords = new List<string>(),
        };

        var flags = service.DetectFlags(commander, card);

        Assert.Contains(flags, f => f.Label == "Shared Subtype");
    }

    [Fact]
    public void DetectFlags_ReturnMultipleFlags_WhenMultipleSynergyRulesTrigger()
    {
        var service = new SynergyFlagService();

        var commander = new Card
        {
            Id = "cmdr-4",
            Name = "Commander",
            Subtypes = new List<string>{"Eldrazi","Titan"},
            Keywords = new List<string>{"Annihalator"},
            KeywordActions = new List<string>{"Create"},
            AbilityWords = new List<string>{"Add"},
            RulesText = "Add an Eldrazi Scion."
        };

        var card = new Card
        {
            Id = "card-4",
            Name = "Gem Card 4",
            Subtypes = new List<string>{"Eldrazi","Scion"},
            Keywords = new List<string>{"Annihalator"},
            KeywordActions = new List<string>{"Create"},
            AbilityWords = new List<string>{"Sacrifice"},
            RulesText = "Sacrifice for 1 Mana"
        };

        var expectedLabels = new[]
        {
            "Shared Subtype",
            "Keyword Alignment",
            "Action Synergy",
            "Resource Engine"
        };

        var flags = service.DetectFlags(commander, card);

        var actualLabels = flags.Select(f => f.Label).ToList();

        foreach (var expectedLabel in expectedLabels)
        {
            Assert.Contains(expectedLabel, actualLabels);
        }

        foreach (var expectedLabel in expectedLabels)
        {
            Assert.Equal(1, actualLabels.Count(label => label == expectedLabel));
        }
    }
    [Fact]
    public void DetectFlags_AddsResourceEngine_WhenRulesTextContainsExactCaseAddAndSacrifice()
    {
    var service = new SynergyFlagService();

    var commander = new Card
    {
        Id = "cmdr-c1-1",
        Name = "Commander",
        RulesText = "Add one mana of any color."
    };

    var card = new Card
    {
        Id = "card-c1-1",
        Name = "Gem",
        RulesText = "Sacrifice another creature."
    };

    var flags = service.DetectFlags(commander, card);

    Assert.Contains(flags, f => f.Label == "Resource Engine");
    }

    [Fact]
    public void DetectFlags_DoesNotAddResourceEngine_WhenRulesTextUsesDifferentCase()
    {
    var service = new SynergyFlagService();

    var commander = new Card
    {
        Id = "cmdr-c1-2",
        Name = "Commander",
        RulesText = "add one mana of any color."
    };

    var card = new Card
    {
        Id = "card-c1-2",
        Name = "Gem",
        RulesText = "sacrifice another creature."
    };

    var flags = service.DetectFlags(commander, card);

    Assert.DoesNotContain(flags, f => f.Label == "Resource Engine");
    }

    [Fact]
    public void DetectFlags_DoesNotAddResourceEngine_WhenSemanticsMatchButLiteralTermsDoNot()
    {
    var service = new SynergyFlagService();

    var commander = new Card
    {
        Id = "cmdr-c1-3",
        Name = "Commander",
        RulesText = "Generate one mana of any color."
    };

    var card = new Card
    {
        Id = "card-c1-3",
        Name = "Gem",
        RulesText = "Exile this creature: create mana."
    };

    var flags = service.DetectFlags(commander, card);

    Assert.DoesNotContain(flags, f => f.Label == "Resource Engine");
    }

    [Fact]
    public void DetectFlags_DoesNotAddKeywordAlignment_WhenKeywordCaseDiffers()
    {
    var service = new SynergyFlagService();

    var commander = new Card
    {
        Id = "cmdr-c1-4",
        Name = "Commander",
        Keywords = new List<string> { "Flying" }
    };

    var card = new Card
    {
        Id = "card-c1-4",
        Name = "Gem",
        Keywords = new List<string> { "flying" }
    };

    var flags = service.DetectFlags(commander, card);

    Assert.DoesNotContain(flags, f => f.Label == "Keyword Alignment");
    }
}
