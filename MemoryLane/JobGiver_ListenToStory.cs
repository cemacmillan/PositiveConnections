using Verse;
using Verse.AI;
using RimWorld;

namespace DIL_PositiveConnections
{
     public class JobGiver_ListenToStory : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.mindState.duty != null && pawn.mindState.duty.def == PositiveConnectionsDutyDefOf.DIL_Duty_Storytelling)
            {
                Log.Message($"[DIL_PositiveConnections] {pawn.LabelShort} is being assigned the ListenToStory job.");
                return JobMaker.MakeJob(PositiveConnectionsJobDefOf.ListenToStory, pawn.mindState.duty.focus.Cell);
            }
            Log.Warning($"[DIL_PositiveConnections] {pawn.LabelShort} could not be assigned the ListenToStory job due to missing or incorrect duty.");
            return null;
        }
    }
}