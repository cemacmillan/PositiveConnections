using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace DIL_PositiveConnections
{
    public static class StorytellingUtility
    {
        public static GatheringDef StorytellingDef = DefDatabase<GatheringDef>.GetNamed("DIL_Gathering_Storytelling");

       private static bool storytellingActive = false;

       public static void CheckAndTriggerStorytelling()
       {
           // Ensure storytelling isn't already active
           if (storytellingActive)
           {
               return;
           }

           int hour = GenLocalDate.HourOfDay(Find.CurrentMap);

           if (hour < 6 || hour >= 20)
           {
               var idleColonists = Find.CurrentMap.mapPawns.FreeColonists.Where(p => p.jobs.curJob == null || p.jobs.curJob.def == JobDefOf.Wait);
               var gatheringSpots = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(IsSocialGatheringSpot);

               foreach (var spot in gatheringSpots)
               {
                   var nearbyColonists = idleColonists.Where(p => p.Position.DistanceToSquared(spot.Position) <= 49);

                   if (nearbyColonists.Count() > 2)
                   {
                       // Set the flag to prevent triggering another event
                       storytellingActive = true;
                       TriggerStorytelling(spot, nearbyColonists, StorytellingDef);
                       return;
                   }
               }
           }
       }
       
       public static void TriggerStorytelling(Thing gatheringSpot, IEnumerable<Pawn> storytellers, GatheringDef gatheringDef)
       {
           if (storytellers.Any(p => p.GetLord() != null))
           {
               Log.Warning("A storytelling event is already active, not triggering another.");
               return;
           }

           var organizer = storytellers.FirstOrDefault();
           var lordJob = new LordJob_Joinable_Storytelling(gatheringSpot.Position, organizer, gatheringDef);
           LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, gatheringSpot.Map, storytellers);

           Log.Message($"Storytelling session started at {gatheringSpot.Label}.");
       }

        public static bool IsSocialGatheringSpot(Thing thing)
        {
            // Check if the thing has a CompGatherSpot component
            return thing.TryGetComp<CompGatherSpot>() != null;
        }

      
    }
}