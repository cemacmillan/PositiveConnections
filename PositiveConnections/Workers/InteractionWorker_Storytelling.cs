using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_Storytelling : InteractionWorker
    {
        public static event System.Action<Pawn, float, string, int> OnPositiveInteraction;

        private PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // Ensure the interaction happens only between colonists in the same faction
            if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
            {
                return 0f;
            }

            // The initiator must have a minimum social skill level to tell a story
            if (initiator.skills.GetSkill(SkillDefOf.Social).Level < PositiveConnectionsTuning.MinSocialSkillRequired_Storytelling)
            {
                return 0f;
            }

            // Ensure recipient is within storytelling range
            if (!IsInStorytellingRange(initiator, recipient))
            {
                return 0f;
            }

            // Ensure there are sufficient listeners
            if (!HasSufficientListeners(initiator))
            {
                return 0f;
            }

            // Calculate the base weight
            float finalWeight = PositiveConnectionsTuning.BaseSelectionWeight_Storytelling;

            // Apply modifiers based on the custom Den room role and the presence of a campfire
            Room room = initiator.GetRoom(RegionType.Set_All);
            if (room != null)
            {
                RoomRoleDef customDenRole = DefDatabase<RoomRoleDef>.GetNamed("DIL_PurposeSpaces_Den", false);
                if (customDenRole != null && room.Role == customDenRole)
                {
                    finalWeight *= PositiveConnectionsTuning.DenRoomRoleModifier;
                }

                if (room.ContainedAndAdjacentThings.Any(thing => thing.def == ThingDefOf.Campfire))
                {
                    finalWeight *= PositiveConnectionsTuning.CampfireModifier;
                }
            }

            // Log the final weight if logging is enabled
            if (modSettings.EnableLogging)
            {
                string logMessage = $"<color=#00FF7F>[Positive Connections]</color> StorytellingInteraction - Weight: {finalWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
                Log.Message(logMessage);
            }

            return finalWeight;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Initialize output parameters to null
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Apply thoughts to the storyteller (initiator) and the audience (recipient)
            ApplyStorytellingThoughts(initiator, recipient);

            // Generate a storytelling message
            string storytellingMessage = $"{initiator.LabelShort} told a story to {recipient.LabelShort}.";

            if (!modSettings.DisableAllMessages)
            {
                Messages.Message(storytellingMessage, new LookTargets(new Pawn[] { initiator, recipient }), MessageTypeDefOf.PositiveEvent);
            }

            OnPositiveInteraction?.Invoke(initiator, 0.25f, "PositiveInteraction", (int)ExperienceValency.Positive);
        }

        private bool IsInStorytellingRange(Pawn initiator, Pawn recipient)
        {
            return (initiator.Position - recipient.Position).LengthHorizontalSquared <= PositiveConnectionsTuning.StorytellingRange * PositiveConnectionsTuning.StorytellingRange;
        }

        private bool HasSufficientListeners(Pawn initiator)
        {
            int listenerCount = 0;

            // Count pawns within the storytelling range
            foreach (Pawn pawn in initiator.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn != initiator && pawn.Faction == Faction.OfPlayer && IsInStorytellingRange(initiator, pawn))
                {
                    listenerCount++;
                }
            }

            // Require at least 2 listeners (excluding the storyteller)
            return listenerCount >= 2;
        }

        private void ApplyStorytellingThoughts(Pawn initiator, Pawn recipient)
        {
            // Apply a positive thought to the storyteller (initiator)
            Thought_Memory storyTellerThought = (Thought_Memory)ThoughtMaker.MakeThought(PositiveConnectionsThoughtDefOf.DIL_Thought_StorytellingOrganizer);
            initiator.needs.mood.thoughts.memories.TryGainMemory(storyTellerThought);

            // Apply a positive thought to the audience/subject (recipient)
            Thought_Memory listenerThought = (Thought_Memory)ThoughtMaker.MakeThought(PositiveConnectionsThoughtDefOf.DIL_Thought_StorytellingAttendee);
            recipient.needs.mood.thoughts.memories.TryGainMemory(listenerThought);
        }
    }
}