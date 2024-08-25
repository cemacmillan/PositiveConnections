using Verse;
using Verse.AI;
using RimWorld;

namespace DIL_PositiveConnections
{
    
       public class JobGiver_TellStory : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.mindState.duty != null && pawn.mindState.duty.def == PositiveConnectionsDutyDefOf.DIL_Duty_Storytelling)
            {
                Log.Message($"[DIL_PositiveConnections] {pawn.LabelShort} is being assigned the TellStory job.");
                return JobMaker.MakeJob(PositiveConnectionsJobDefOf.TellStory, pawn.mindState.duty.focus.Cell);
            }
            Log.Warning($"[DIL_PositiveConnections] {pawn.LabelShort} could not be assigned the TellStory job due to missing or incorrect duty.");
            return null;
        }
    }

}