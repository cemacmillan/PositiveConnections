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
        private const int NEGATIVE_RELATIONSHIP_THRESHOLD = -10;
        private const int MINIMUM_SOCIAL_SKILL_REQUIRED = 6;
        private const int MIN_MEDIATION_BONUS = 6;
        private const int MAX_MEDIATION_BONUS = 20;
        private float BASE_SELECTION_WEIGHT = 0.01f;
        //private float BASE_SELECTION_WEIGHT = 0.9f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // If the initiator's social skill is less than the minimum required, this interaction should not occur.
            if (initiator.skills.GetSkill(SkillDefOf.Social).Level < MINIMUM_SOCIAL_SKILL_REQUIRED)
            {
                return 0f;
            }

            // Get list of all colony pawns
            var colonyPawns = recipient.Map.mapPawns.FreeColonists;

            // Look for potential conflicts that the initiator could mediate.
            foreach (var pawn in colonyPawns)
            {
                if (recipient != pawn && recipient.relations.OpinionOf(pawn) < NEGATIVE_RELATIONSHIP_THRESHOLD)
                {
                    return BASE_SELECTION_WEIGHT;
                }
            }

            // If there are no potential conflicts, return a weight of zero.
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

            // If no suitable conflict pawn is found, return early.
            if (conflictPawn == null)
            {
                return;
            }

            // Calculate the outcome based on the initiator's social skill
            var mediationBonus = CalculateMediationBonus(initiator);

            // Apply the mediation bonus to the conflict pawn and recipient
            ApplyMediationOutcome(initiator, recipient, conflictPawn, mediationBonus);

            // Log the interaction for debugging purposes
            Log.Message($"[Positive Connections] {initiator.Name.ToStringShort} mediated a conflict between {recipient.Name.ToStringShort} and {conflictPawn.Name.ToStringShort}. Their relationship improved by {mediationBonus}.");
            // Tell the player about it duh
            string mediationMessage = string.Format("{0} mediated a conflict between {1} and {2}. Their relationship improved by {3}.", initiator.LabelShort, recipient.LabelShort, conflictPawn.LabelShort, mediationBonus);
            Messages.Message(mediationMessage, new LookTargets(new Pawn[] { initiator, recipient, conflictPawn }), MessageTypeDefOf.PositiveEvent);


        }



        private Pawn FindConflictPawn(Pawn recipient)
        {
            // Get list of all colony pawns
            var colonyPawns = recipient.Map.mapPawns.FreeColonists;

            // Find a pawn with which the recipient has a negative relationship
            foreach (var pawn in colonyPawns)
            {
                if (recipient != pawn && recipient.relations.OpinionOf(pawn) < NEGATIVE_RELATIONSHIP_THRESHOLD)
                {
                    return pawn;
                }
            }

            // If there are no potential conflicts, return null.
            return null;
        }


        private int CalculateMediationBonus(Pawn initiator)
        {
            // Use initiator's social skill to calculate bonus
            int socialSkill = initiator.skills.GetSkill(SkillDefOf.Social).Level;
            float randomFactor = Rand.Value; // Returns a random float between 0 and 1
            int mediationBonus = (int)(MIN_MEDIATION_BONUS + (MAX_MEDIATION_BONUS - MIN_MEDIATION_BONUS) * randomFactor * socialSkill / 20f);

            return mediationBonus; // Return raw mediationBonus
        }

        private void ApplyMediationOutcome(Pawn initiator, Pawn recipient, Pawn conflictPawn, int mediationBonus)
        {
           

            // Convert mediationBonus into a range from 0 to 2
            int convertedBonus = (int)Mathf.Round(mediationBonus / (MAX_MEDIATION_BONUS / 2f));
            convertedBonus = Mathf.Clamp(convertedBonus, 0, 2); // Ensure the result is within the range 0 to 2
           
            // Create a memory of the successful mediation
            Thought_Memory memoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
            Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
            Thought_Memory memoryConflictPawn = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
           
            // The moodPowerFactor is fixed, but we can modify the base mood effect directly
            memoryInitiator.SetForcedStage(convertedBonus);
            memoryRecipient.SetForcedStage(convertedBonus);
            memoryConflictPawn.SetForcedStage(convertedBonus);

            // Add the memory to the initiator, recipient, and conflictPawn
            initiator.needs.mood.thoughts.memories.TryGainMemory(memoryInitiator);
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient);
            conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryConflictPawn);
          
            Thought_Memory memoryShared = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOfPositiveConnections.DIL_InMediationWith);
            memoryShared.SetForcedStage(0); // Set the thought stage based on mediation bonus
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryShared, conflictPawn);
            conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryShared, recipient);


          
        }






    }
}

