using System;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_SharedPassion : InteractionWorker
    {
        private const float BaseSelectionWeight = 0.005f;
        private const int PassionLevelFactor = 2;  // Increase selection weight based on passion level

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // Both pawns should have a shared skill with burning passion
            var sharedPassion = initiator.skills.skills
                .FirstOrDefault(s => recipient.skills.skills.Any(rs => rs.def == s.def && (int)rs.passion > 0 && (int)s.passion > 0));

            if (sharedPassion == null)
            {
                return 0f;
            }

            float baseWeight = (initiator.needs.mood.CurLevel + recipient.needs.mood.CurLevel) / 2 * BaseSelectionWeight;

            if (initiator.Faction == recipient.Faction)
            {
                // Increase the weight based on passion level
                return baseWeight * ((int)sharedPassion.passion * PassionLevelFactor);
            }
            else
            {
                return baseWeight * 0.2f; // 1/5 as likely
            }
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Find the shared passion
            var sharedPassion = initiator.skills.skills
                .FirstOrDefault(s => recipient.skills.skills.Any(rs => rs.def == s.def && (int)rs.passion > 0 && (int)s.passion > 0));

            // Generate message and gain memory of the shared activity
            if (sharedPassion != null)
            {
                //string passionMessage = $"{initiator.Name.ToStringShort} and {recipient.Name.ToStringShort} enjoyed {sharedPassion.def.skillLabel} together";
                string passionMessage = PositiveConnectionsUtility.GenerateSharedPassionMessage(initiator, recipient, sharedPassion);

                Messages.Message(passionMessage, recipient, MessageTypeDefOf.PositiveEvent);


                Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SharedPassionActivity"));
                memory.moodPowerFactor = 1f;
                initiator.needs.mood.thoughts.memories.TryGainMemory(memory);
                recipient.needs.mood.thoughts.memories.TryGainMemory(memory);
            }

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Update relationship between initiator and recipient
            Faction factionA = initiator.Faction;
            Faction factionB = recipient.Faction;

            if (factionA != null && factionB != null && factionA != factionB)
            {
                PositiveConnectionsUtility.ChangeFactionRelations(factionA, factionB, 10);
            }
        }
    }

}

