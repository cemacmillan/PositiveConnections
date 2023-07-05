using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace dIl_PositiveConnections
{
    public static class PositiveConnectionsUtility
    {
        public static void ChangeFactionRelations(Faction factionA, Faction factionB, int goodwillChange)
        {
            if (goodwillChange == 0)
            {
                goodwillChange = Rand.RangeInclusive(1, 15);
            }
            else
            {
                goodwillChange = Mathf.Clamp(goodwillChange, -100, 100);
            }

            FactionRelation factionRelationA = factionA.RelationWith(factionB);
            FactionRelation factionRelationB = factionB.RelationWith(factionA);

            factionRelationA.baseGoodwill = Mathf.Clamp(factionRelationA.baseGoodwill + goodwillChange, -100, 100);
            factionRelationB.baseGoodwill = factionRelationA.baseGoodwill;

            factionRelationA.CheckKindThresholds(factionA, canSendLetter: false, null, GlobalTargetInfo.Invalid, out _);
            factionRelationB.CheckKindThresholds(factionB, canSendLetter: false, null, GlobalTargetInfo.Invalid, out _);
        }

        public static string GenerateComplimentMessage(Pawn initiator, Pawn recipient)
        {
            string[] compliments = { "social skills", "hard work", "bravery", "intelligence", "grace", "patience" };
            string randomCompliment = compliments.RandomElement();

            return $"{initiator.Name} compliments {recipient.Name} on their {randomCompliment}!";
        }
    }
}

