using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace DIL_PositiveConnections
{
    public class GatheringWorker_Storytelling : GatheringWorker
    {
        protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
        {
            return new LordJob_Joinable_Storytelling(spot, organizer, def);
        }

        protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
        {
            bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer);
            IEnumerable<Thing> gatheringSpots = organizer.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Where(t => t.TryGetComp<CompGatherSpot>() != null);

            if (gatheringSpots.TryRandomElement(out var result))
            {
                spot = result.InteractionCell;
                return true;
            }

            spot = IntVec3.Invalid;
            return false;
        }

        public override bool CanExecute(Map map, Pawn organizer = null)
        {
            // Check if the map is valid before proceeding
            if (map == null)
            {
                Log.Warning("[DIL_PositiveConnections] CanExecute called with null map.");
                return false;
            }

            // If no organizer is provided, try to find one
            if (organizer == null)
            {
                organizer = FindOrganizer(map);
                if (organizer == null)
                {
                    Log.Warning("[DIL_PositiveConnections] CanExecute called with null organizer and no suitable organizer found.");
                    return false;
                }
            }

            // Ensure the map is fully initialized before trying to execute a gathering
            if (!map.IsPlayerHome || map.ParentFaction == null || !map.ParentFaction.IsPlayer)
            {
                Log.Warning("[DIL_PositiveConnections] CanExecute called on non-player home map.");
                return false;
            }

            return base.CanExecute(map, organizer);
        }

        // Override SendLetter to send a message instead
        protected override void SendLetter(IntVec3 spot, Pawn organizer)
        {
            string messageText = def.letterText.Formatted(organizer.Named("ORGANIZER"));
            Messages.Message(messageText, new TargetInfo(spot, organizer.Map), MessageTypeDefOf.PositiveEvent);
        }

    }




}