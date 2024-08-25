// JobDriver_TellStory.cs
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace DIL_PositiveConnections
{
    public class JobDriver_TellStory : JobDriver
    {
        private const TargetIndex SpotInd = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool result = pawn.Reserve(job.GetTarget(SpotInd), job, errorOnFailed: errorOnFailed);
            Log.Message($"[DIL_PositiveConnections] {pawn.Name.ToStringShort} reserved spot for telling story: {result}");
            return result;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(SpotInd);

            yield return Toils_Goto.GotoCell(SpotInd, PathEndMode.OnCell);

            Toil tellStory = new Toil
            {
                initAction = () =>
                {
                    Log.Message($"[DIL_PositiveConnections] {pawn.Name.ToStringShort} started telling a story at {TargetA.Cell}");
                },
                tickAction = () =>
                {
                    pawn.rotationTracker.FaceCell(job.GetTarget(SpotInd).Cell);
                    pawn.GainComfortFromCellIfPossible();
                    if (pawn.IsHashIntervalTick(100))
                    {
                        pawn.jobs.CheckForJobOverride();
                        Log.Message($"[DIL_PositiveConnections] {pawn.Name.ToStringShort} is telling a story...");
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never,
                handlingFacing = true
            };
            yield return tellStory;
        }
    }
}