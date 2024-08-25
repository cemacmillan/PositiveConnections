using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;
using System;
using UnityEngine;

namespace DIL_PositiveConnections
{
    public class LordJob_Joinable_Storytelling : LordJob_Joinable_Gathering
    {
        public override bool AllowStartNewGatherings => false;
        //private int durationTicks;

        //public int DurationTicks => durationTicks;


        // Mind Matters Bridge
        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        // Using DefOf references directly
        protected virtual ThoughtDef OrganizerThought => PositiveConnectionsThoughtDefOf.DIL_Thought_StorytellingOrganizer;
        protected virtual ThoughtDef AttendeeThought => PositiveConnectionsThoughtDefOf.DIL_Thought_StorytellingAttendee;
        protected virtual TaleDef OrganizerTale => PositiveConnectionsTaleDefOf.DIL_Tale_StorytellingOrganizer;
        protected virtual TaleDef AttendeeTale => PositiveConnectionsTaleDefOf.DIL_Tale_StorytellingAttendee;

        public LordJob_Joinable_Storytelling()
        {
            Log.Message("[DIL_PositiveConnections] Default constructor called for LordJob_Joinable_Storytelling. This is definitely a mistake.");
        }

        public LordJob_Joinable_Storytelling(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
            : base(spot, organizer, gatheringDef)
        {
            durationTicks = Rand.RangeInclusive(500, 1000); 
            Log.Message($"[DIL_PositiveConnections] Constructor called for LordJob_Joinable_Storytelling with durationTicks = {durationTicks}");
        }

        public override string GetReport(Pawn pawn)
        {
            Log.Message($"[DIL_PositiveConnections] GetReport called for pawn {pawn.LabelShort}");
            //return "LordReportAttendingStorytelling".Translate();
            return "LordReportAttendingParty".Translate();
        }

        protected override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
        {
            Log.Message($"[DIL_PositiveConnections] CreateGatheringToil called with spot = {spot}, organizer = {organizer.LabelShort}, gatheringDef = {gatheringDef.defName}");
            return new LordToil_Storytelling(spot, gatheringDef);
        }

/*
        protected override LordToil CreateGatheringToil(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
        {
            return new LordToil_Party(spot, gatheringDef);
        }
*/

        public override StateGraph CreateGraph()
        {
            Log.Message("[DIL_PositiveConnections] CreateGraph called for storytelling.");
            StateGraph stateGraph = new StateGraph();

            Log.Message("[DIL_PositiveConnections] Creating storytelling toil.");
            var storytellingToil = CreateGatheringToil(spot, organizer, gatheringDef);

            // Log the creation of the storytelling toil
            if (storytellingToil == null)
            {
                Log.Error("[DIL_PositiveConnections] storytellingToil is null in CreateGraph. This is definitely wrong.");
            }

            stateGraph.AddToil(storytellingToil);

            Log.Message("[DIL_PositiveConnections] Creating lordToilEnd.");
            var lordToilEnd = new LordToil_End();
            stateGraph.AddToil(lordToilEnd);

            Log.Message("[DIL_PositiveConnections] Adding transition for ShouldBeCalledOff.");
            var transition = new Transition(storytellingToil, lordToilEnd);
            //transition.AddTrigger(new Trigger_TickCondition(ShouldBeCalledOff));
            transition.AddTrigger(new Trigger_TickCondition(() => ShouldBeCalledOff()));
            stateGraph.AddTransition(transition);

            Log.Message("[DIL_PositiveConnections] Adding timeout transition.");
            var timeoutTransition = new Transition(storytellingToil, lordToilEnd);
            timeoutTransition.AddTrigger(GetTimeoutTrigger());
            stateGraph.AddTransition(timeoutTransition);

            Log.Message("[DIL_PositiveConnections] StateGraph created for storytelling.");
            return stateGraph;
        }

      
        protected override Trigger_TicksPassed GetTimeoutTrigger()
        {
            Log.Message($"[DIL_PositiveConnections] GetTimeoutTrigger called with durationTicks = {durationTicks}");
            return new Trigger_TicksPassed(durationTicks);
        }

        private void ApplyOutcome(LordToil_Storytelling toil)
        {
            Log.Message("[DIL_PositiveConnections] ApplyOutcome called for storytelling.");
            if (lord == null)
            {
                Log.Error("[DIL_PositiveConnections] lord is null at ApplyOutcome.");
                return;  // Avoid further null reference issues
            }

            var ownedPawns = lord.ownedPawns;
            var lordToilData = (LordToilData_Gathering)toil.data;

            for (int i = 0; i < ownedPawns.Count; i++)
            {
                var pawn = ownedPawns[i];
                bool isOrganizer = pawn == organizer;

                if (lordToilData.presentForTicks.TryGetValue(pawn, out int value) && value > 0)
                {
                    var thoughtDef = isOrganizer ? OrganizerThought : AttendeeThought;
                    float moodPowerFactor = Mathf.Min((float)value / (float)durationTicks, 1f);
                    var thoughtMemory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
                    thoughtMemory.moodPowerFactor = moodPowerFactor;
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtMemory);

                    TaleRecorder.RecordTale(isOrganizer ? OrganizerTale : AttendeeTale, pawn, organizer);

                    Log.Message($"[DIL_PositiveConnections] {pawn.LabelShort} received thought {thoughtDef.defName} and tale {(isOrganizer ? OrganizerTale.defName : AttendeeTale.defName)}");

                    // Invoke Mind Matters positive interaction event
                    OnPositiveInteraction?.Invoke(pawn, 0.1f, "Storytelling", (int)ExperienceValency.Positive);
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Log.Message($"[DIL_PositiveConnections] ExposeData called for LordJob_Joinable_Storytelling. durationTicks = {durationTicks}");
            Scribe_Values.Look(ref durationTicks, "durationTicks", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && gatheringDef == null)
            {
                gatheringDef = DefDatabase<GatheringDef>.GetNamed("DIL_Gathering_Storytelling");
            }
        }
    }
}