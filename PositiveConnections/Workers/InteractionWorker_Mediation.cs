using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_Mediation : InteractionWorker
    {
        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        private PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // Only consider colonists
            if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
            {
                return 0f;
            }

            // If the initiator's social skill is less than the minimum required, this interaction should not occur
            if (initiator.skills.GetSkill(SkillDefOf.Social).Level < PositiveConnectionsTuning.MinSocialSkillRequired_Mediation || initiator.Faction != recipient.Faction)
            {
                return 0f;
            }

            // Get the list of all colony pawns
            IEnumerable<Pawn> colonyPawns = recipient.Map.mapPawns.FreeColonists;

            // Look for potential conflicts that the initiator could mediate
            foreach (Pawn pawn in colonyPawns)
            {
                if (recipient != pawn && recipient.relations.OpinionOf(pawn) < PositiveConnectionsTuning.NegativeRelationshipThreshold_Mediation)
                {
                    // Calculate the base weight using the utility method and tuning class
                    float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(initiator, PositiveConnectionsTuning.BaseSelectionWeight_Mediation, modSettings, applyMoodModifier: false);

                    return baseWeight;
                }
            }

            // If there are no potential conflicts, return a weight of zero
            return 0f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Initialize output parameters to null
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Locate a suitable conflict for the initiator to mediate
            var conflictPawn = FindConflictPawn(recipient);

            // If no suitable conflict pawn is found, return early
            if (conflictPawn == null)
            {
                return;
            }

            // Calculate the outcome based on the initiator's social skill
            var mediationBonus = CalculateMediationBonus(initiator);

            // Apply the mediation bonus to the conflict pawn and recipient
            ApplyMediationOutcome(initiator, recipient, conflictPawn, mediationBonus);

            // Generate a mediation message
            string mediationMessage = string.Format("{0} mediated a conflict between {1} and {2}.", initiator.LabelShort, recipient.LabelShort, conflictPawn.LabelShort);

            if (!modSettings.DisableAllMessages)
            {
                Messages.Message(mediationMessage, new LookTargets(new Pawn[] { initiator, recipient, conflictPawn }), MessageTypeDefOf.PositiveEvent);
            }

            OnPositiveInteraction?.Invoke(initiator, 0.25f, "PositiveInteraction", (int)ExperienceValency.Positive);

            // New logging
            if (modSettings.EnableLogging)
            {
                string logMessage = $"<color=#00FF7F>[Positive Connections]</color> Mediation - Weight: {RandomSelectionWeight(initiator, recipient)} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}, ConflictPawn: {conflictPawn.Name.ToStringShort}, MediationBonus: {mediationBonus}";
                Log.Message(logMessage);
            }
        }

        private Pawn FindConflictPawn(Pawn recipient)
        {
            var colonyPawns = recipient.Map.mapPawns.FreeColonists;

            foreach (var pawn in colonyPawns)
            {
                if (recipient != pawn && recipient.relations.OpinionOf(pawn) < PositiveConnectionsTuning.NegativeRelationshipThreshold_Mediation)
                {
                    return pawn;
                }
            }

            return null;
        }

        private int CalculateMediationBonus(Pawn initiator)
        {
            int socialSkill = initiator.skills.GetSkill(SkillDefOf.Social).Level;
            float randomFactor = Rand.Value;
            int mediationBonus = (int)(PositiveConnectionsTuning.MinMediationBonus + (PositiveConnectionsTuning.MaxMediationBonus - PositiveConnectionsTuning.MinMediationBonus) * randomFactor * socialSkill / 20f);

            return mediationBonus;
        }

        private void ApplyMediationOutcome(Pawn initiator, Pawn recipient, Pawn conflictPawn, int mediationBonus)
        {
            int convertedBonus = (int)Mathf.Round(mediationBonus / (PositiveConnectionsTuning.MaxMediationBonus / 2f));
            convertedBonus = Mathf.Clamp(convertedBonus, 0, 2);

            Thought_Memory memoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
            Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
            Thought_Memory memoryConflictPawn = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));

            memoryInitiator.SetForcedStage(convertedBonus);
            memoryRecipient.SetForcedStage(convertedBonus);
            memoryConflictPawn.SetForcedStage(convertedBonus);

            initiator.needs.mood.thoughts.memories.TryGainMemory(memoryInitiator);
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient);
            conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryConflictPawn);

            Thought_Memory memoryShared = (Thought_Memory)ThoughtMaker.MakeThought(PositiveConnectionsThoughtDefOf.DIL_InMediationWith);
            memoryShared.SetForcedStage(0);
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryShared, conflictPawn);
            conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryShared, recipient);
        }
    }
}