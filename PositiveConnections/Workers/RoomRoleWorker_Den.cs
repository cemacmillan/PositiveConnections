using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace DIL_PositiveConnections
{
    public class RoomRoleWorker_Den : RoomRoleWorker
    {
        private static HashSet<Region> tmpRegions = new HashSet<Region>();
        private static HashSet<Building_Door> tmpDoors = new HashSet<Building_Door>();

        public override float GetScore(Room room)
        {
            int bedCount = 0;
            bool hasRecreationalFacility = false;

            var allContainedThings = room.ContainedAndAdjacentThings;

            // Count beds and check for recreational facilities
            foreach (var thing in allContainedThings)
            {
                if (thing is Building_Bed bed)
                {
                    if (bed.def.building.bed_humanlike && !bed.ForPrisoners)
                    {
                        bedCount++;
                    }
                }
                if (IsJoyGiver(thing))
                {
                    hasRecreationalFacility = true;
                }
            }

            // Count the number of doors
            int doorCount = CountDoors(room);

            bool hasSingleEntrance = doorCount == 1;

            // Calculate score
            if (bedCount > 1 && hasSingleEntrance && hasRecreationalFacility)
            {
                return 100f; // High score indicating it is a den
            }
            return 0f;
        }

        private int CountDoors(Room room)
        {
            tmpRegions.Clear();
            tmpDoors.Clear();

            foreach (Region region in room.Regions)
            {
                foreach (RegionLink link in region.links)
                {
                    Region otherRegion = link.GetOtherRegion(region);
                    if (otherRegion.type != RegionType.Portal || !tmpRegions.Add(otherRegion))
                    {
                        continue;
                    }
                    Building_Door door = otherRegion.door;
                    for (int i = 0; i < otherRegion.links.Count; i++)
                    {
                        Region regionA = otherRegion.links[i].RegionA;
                        Region regionB = otherRegion.links[i].RegionB;
                        if ((regionA.Room != room && regionA != otherRegion && regionA.door != door) || (regionB.Room != room && regionB != otherRegion && regionB.door != door))
                        {
                            tmpDoors.Add(door);
                            break;
                        }
                    }
                }
            }

            return tmpDoors.Count;
        }

        private bool IsJoyGiver(Thing thing)
        {
            List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
            for (int j = 0; j < allDefsListForReading.Count; j++)
            {
                if (allDefsListForReading[j].thingDefs != null && allDefsListForReading[j].thingDefs.Contains(thing.def))
                {
                    return true;
                }
            }
            return false;
        }
    }
}