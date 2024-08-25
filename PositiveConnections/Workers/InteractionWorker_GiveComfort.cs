using System;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_GiveComfort : InteractionWorker
    {
        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        private PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // Ensure the initiator and recipient are relevant to the player's colony
            if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
            {
                return 0f;
            }

            // Check if the initiator has the required social skill or the Kind trait
            if (initiator.skills.GetSkill(SkillDefOf.Social).Level < PositiveConnectionsTuning.MinSocialSkillRequired_GiveComfort
            && !initiator.story.traits.HasTrait(TraitDefOf.Kind))
            {
                return 0f;
            }

            // Check if the recipient's mood is low enough
            if (recipient.needs.mood.CurLevelPercentage * 100 >= PositiveConnectionsTuning.MinMoodForComfort)
            {
                return 0f;
            }

            // Calculate the base weight using the utility method and tuning class, without applying the mood modifier
            float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(initiator, PositiveConnectionsTuning.BaseSelectionWeight_GiveComfort, modSettings, applyMoodModifier: false);

            // Adjust weight for cross-faction interactions
            if (recipient.Faction != initiator.Faction)
            {
                baseWeight *= 0.05f;
            }

            return baseWeight;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Initialize output parameters to null
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Create a memory of the comforting interaction
            Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_ComfortReceived"));

            // Add the memory to the recipient
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient, initiator);

            // Notify the player
            string comfortMessage = string.Format("{0} comforted {1}.", initiator.LabelShort, recipient.LabelShort);

            if (!modSettings.DisableAllMessages)
            {
                Messages.Message(comfortMessage, new LookTargets(new Pawn[] { initiator, recipient }), MessageTypeDefOf.PositiveEvent);
            }

            OnPositiveInteraction?.Invoke(initiator, 0.1f, "PositiveInteraction", (int)ExperienceValency.Positive);

            // New logging
            if (modSettings.EnableLogging)
            {
                string logMessage = $"<color=#00FF7F>[Positive Connections]</color> GiveComfort - Weight: {RandomSelectionWeight(initiator, recipient)} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
                Log.Message(logMessage);
            }
        }
    }
}