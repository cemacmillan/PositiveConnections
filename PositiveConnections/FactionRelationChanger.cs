using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace dIl_PositiveConnections
{
    public static class FactionRelationsChanger
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
    }
}

