using HiddenGemShared.Entities;
using HiddenGemShared.Models;

namespace HiddenGemBLL.Interfaces;

public interface ISynergyFlagService
{
    List<SynergyFlag> DetectFlags(Card commander, Card card);
}