//  JobDriver_ListenToStory.cs

using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace DIL_PositiveConnections
{
    public class JobDriver_ListenToStory : JobDriver
    {
        private const TargetIndex SpotInd = TargetIndex.A;
        private const TargetIndex WatchTargetInd = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool result = pawn.ReserveSittableOrSpot(job.GetTarget(SpotInd).Cell, job, errorOnFailed);
            Log.Message($"[DIL_PositiveConnections] {pawn.Name.ToStringShort} reserved spot for spectating storytelling: {result}");
            return result;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (job.GetTarget(TargetIndex.C).HasThing)
            {
                this.EndOnDespawnedOrNull(TargetIndex.C);
            }
            yield return Toils_Goto.GotoCell(SpotInd, PathEndMode.OnCell);
            Toil spectate = new Toil
            {
                initAction = () =>
                {
                    Log.Message($"[DIL_PositiveConnections] {pawn.Name.ToStringShort} started spectating a story at {TargetA.Cell}");
                },
                tickAction = () =>
                {
                    pawn.rotationTracker.FaceCell(job.GetTarget(WatchTargetInd).Cell);
                    pawn.GainComfortFromCellIfPossible();
                    if (pawn.IsHashIntervalTick(100))
                    {
                        pawn.jobs.CheckForJobOverride();
                        Log.Message($"[DIL_PositiveConnections] {pawn.Name.ToStringShort} is spectating a story...");
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never,
                handlingFacing = true
            };
            yield return spectate;
        }
    }
}