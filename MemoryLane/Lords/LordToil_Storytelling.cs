using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace DIL_PositiveConnections
{
    public class LordToil_Storytelling : LordToil_Gathering
    {
        private const int UpdateIntervalTicks = 100;  // Interval to update actions

        public LordToil_Storytelling(IntVec3 spot, GatheringDef gatheringDef)
            : base(spot, gatheringDef)
        {
        }

        public override void UpdateAllDuties()
{
    base.UpdateAllDuties();
    foreach (Pawn pawn in lord.ownedPawns)
    {
        if (pawn == lord.ownedPawns[0])  // Assuming the first pawn is the organizer
        {
            pawn.mindState.duty = new PawnDuty(PositiveConnectionsDutyDefOf.DIL_Duty_Storytelling_Organizer, spot);
        }
        else
        {
            pawn.mindState.duty = new PawnDuty(PositiveConnectionsDutyDefOf.DIL_Duty_Storytelling_Attendee, spot);
        }
    }
}

        public override void LordToilTick()
        {
            base.LordToilTick();
            if (Find.TickManager.TicksGame % UpdateIntervalTicks == 0)
            {
                List<Pawn> ownedPawns = lord.ownedPawns;

                for (int i = 0; i < ownedPawns.Count; i++)
                {
                    Pawn pawn = ownedPawns[i];
                    if (pawn.CurJob == null || pawn.CurJob.def != PositiveConnectionsJobDefOf.TellStory && pawn.CurJob.def != PositiveConnectionsJobDefOf.ListenToStory)
                    {
                        if (pawn == lord.ownedPawns[0])
                        {
                            pawn.jobs.StartJob(new Job(PositiveConnectionsJobDefOf.TellStory, spot), JobCondition.InterruptForced);
                        }
                        else
                        {
                            pawn.jobs.StartJob(new Job(PositiveConnectionsJobDefOf.ListenToStory, spot), JobCondition.InterruptForced);
                        }
                    }
                }
            }
        }
    }
}