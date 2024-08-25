using System;
using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    [DefOf]
    public static class PositiveConnectionsGatheringDefOf
    {

        static PositiveConnectionsGatheringDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GatheringDefOf));
        }

    }

    
}

