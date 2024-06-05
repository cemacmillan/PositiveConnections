using System;
using System.Linq;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_SharedPassion : InteractionWorker
    {
        private const float BaseSelectionWeight = 0.01f; // Increased base weight
        private const int PassionLevelFactor = 2;  // Increase selection weight based on passion level
        private const float NonColonyPawnFactor = 0.05f; // Reduced weight for non-colony pawns
        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        private PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            // Both pawns should have a shared skill with passion
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
                return baseWeight * ((int)sharedPassion.passion * PassionLevelFactor) * modSettings.BaseInteractionFrequency;
            }
            else
            {
                // Reduced weight for interactions with non-colony pawns
                return baseWeight * NonColonyPawnFactor * modSettings.BaseInteractionFrequency;
            }
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Find the shared passion
            var sharedPassion = initiator.skills.skills
                .FirstOrDefault(s => recipient.skills.skills.Any(rs => rs.def == s.def && (int)rs.passion > 0 && (int)s.passion > 0));

            if (sharedPassion != null)
            {
                // Generate a shared passion message using the utility method
                string passionMessage = PositiveConnectionsUtility.GenerateSharedPassionMessage(initiator, recipient, sharedPassion);

                if (!modSettings.DisableAllMessages)
                {
                    Messages.Message(passionMessage, recipient, MessageTypeDefOf.PositiveEvent);
                }

                // Create and gain memory of the shared activity
                Thought_Memory memoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SharedPassionActivity"));
                Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SharedPassionActivity"));

                memoryInitiator.moodPowerFactor = 1f;
                memoryRecipient.moodPowerFactor = 1f;

                initiator.needs.mood.thoughts.memories.TryGainMemory(memoryInitiator, recipient);
                recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient, initiator);

                // Update relationship between initiator and recipient
                Faction factionA = initiator.Faction;
                Faction factionB = recipient.Faction;

                if (factionA != null && factionB != null && factionA != factionB)
                {
                    // Adjust faction relations based on the memory's mood power factor
                    int relationImpact = (int)(memoryInitiator.moodPowerFactor * 10);
                    PositiveConnectionsUtility.ChangeFactionRelations(factionA, factionB, relationImpact);
                }

                OnPositiveInteraction?.Invoke(initiator, 0.1f, "PositiveInteraction", (int)ExperienceValency.Positive);
            }

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
        }
    }
}