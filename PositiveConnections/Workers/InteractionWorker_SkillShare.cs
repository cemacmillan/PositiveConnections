using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_SkillShare : InteractionWorker
    {
        private const float BaseSelectionWeight = 0.005f; // Halved base weight
        private const float NonColonyPawnFactor = 0.2f; // Adjusted factor for non-colony pawns
        private const int SkillDifferenceDivisor = 10; // Increased divisor for skill difference
        private PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();
        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        private static readonly List<string> TeachingMessages = new List<string>
        {
            "{0} taught {1} about {2}.",
            "{0} shared knowledge of {2} with {1}.",
            "{0} instructed {1} in the ways of {2}.",
            "{0} gave a lesson in {2} to {1}.",
            "{0} helped {1} improve their {2} skills.",
            "{0} spent time teaching {1} about {2}.",
            "{0} provided insight into {2} for {1}.",
            "{0} and {1} had a productive session on {2}.",
            "{0} guided {1} in mastering {2}.",
            "{0} and {1} discussed advanced techniques in {2}."
        };

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            // Get the highest skill of the initiator using Verse's MaxBy
            SkillRecord highestSkill = Verse.GenCollection.MaxBy(initiator.skills.skills, s => s.Level);

            // If the recipient's skill level in the same area is less, allow teaching
            if (highestSkill != null && recipient.skills.GetSkill(highestSkill.def).Level < highestSkill.Level)
            {
                int colonySize = initiator.Map.mapPawns.FreeColonistsCount;
                float adjustedFrequency = PositiveConnectionsUtility.AdjustInteractionFrequency(colonySize, modSettings);

                float weight = (highestSkill.Level - recipient.skills.GetSkill(highestSkill.def).Level) / SkillDifferenceDivisor * initiator.needs.mood.CurLevel * BaseSelectionWeight;

                if (initiator.Faction != Faction.OfPlayer || recipient.Faction != Faction.OfPlayer)
                {
                    weight *= NonColonyPawnFactor;
                }

                float finalWeight = weight * adjustedFrequency * modSettings.BaseInteractionFrequency;

                return finalWeight;
            }
            return 0f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            SkillRecord highestSkill = Verse.GenCollection.MaxBy(initiator.skills.skills, s => s.Level);

            if (highestSkill != null)
            {
                recipient.skills.GetSkill(highestSkill.def).Learn(200f);

                string teachingMessage = string.Format(TeachingMessages.RandomElement(), initiator.Name, recipient.Name, highestSkill.def.label);

                if (!modSettings.DisableAllMessages)
                {
                    Messages.Message(teachingMessage, recipient, MessageTypeDefOf.PositiveEvent);
                }

                recipient.needs.mood.thoughts.memories.TryGainMemory(PositiveConnectionsThoughtDefOf.DIL_ReceivedTeaching, initiator);

                OnPositiveInteraction?.Invoke(initiator, 0.2f, "PositiveInteraction", (int)ExperienceValency.Positive);

                // New logging
                if (modSettings.EnableLogging)
                {
                    string logMessage = $"<color=#00FF7F>[Positive Connections]</color> SkillShare - Weight: {RandomSelectionWeight(initiator, recipient)} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}, Skill: {highestSkill.def.defName}";
                    Log.Message(logMessage);
                }
            }

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
        }
    }
}