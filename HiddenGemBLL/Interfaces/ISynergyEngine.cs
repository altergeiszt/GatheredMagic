using HiddenGemShared.Entities;
using HiddenGemBLL.Models;

namespace HiddenGemBLL.Interfaces;

public interface ISynergyEngine
{
    SynergyRelation? ProcessRelationship(Card commander, Card card, DeckStats deckstats);

}