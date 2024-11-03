using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc;

public class InteractionWorker_Storytelling : InteractionWorker
{
    public static event System.Action<Pawn, float, string, int> OnPositiveInteraction;

    private PositiveConnectionsModSettings _modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();
    
    // Encapsulated property for the last computed selection weight
    public float CurrentRandomSelectionWeight { get; private set; } = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        // 1. Ensure the initiator and recipient are relevant to the player's colony
        if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
        {
            return 0f;
        }
        

        // 1.5 Check if the recipient is sick of hearing stories
        ThoughtDef storytellingAttendeeThought = ThoughtDef.Named("DIL_Thought_StorytellingAttendee");
        if (PositiveConnectionsUtility.HasLimitedMemories(recipient, storytellingAttendeeThought,
                PositiveConnectionsTuning.MaxListenings_Storytelling))
        {
            return 0f;
        }

        // 2. Condition specific to Storytelling: Initiator's social skill level >= MinSocialSkillRequired_Storytelling
        if (initiator.skills.GetSkill(SkillDefOf.Social).Level < PositiveConnectionsTuning.MinSocialSkillRequired_Storytelling)
        {
            return 0f;
        }
        
        // 2.5 Ensure there are listeners
        if (!HasSufficientListeners(initiator))
        {
            return 0f;
        }
        
        // 3. Calculate the base weight
        float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(initiator, PositiveConnectionsTuning.BaseSelectionWeight_Storytelling, _modSettings);

        // 4. Apply modifiers
        // Apply modifiers based on the custom Den room role and the presence of a campfire
        Room room = initiator.GetRoom(RegionType.Set_All);
        if (room != null)
        {
            RoomRoleDef customDenRole = DefDatabase<RoomRoleDef>.GetNamed("DIL_PurposeSpaces_Den", false);
            if (customDenRole != null && room.Role == customDenRole)
            {
                baseWeight *= PositiveConnectionsTuning.DenRoomRoleModifier;
            }

            if (room.ContainedAndAdjacentThings.Any(thing => thing.def == ThingDefOf.Campfire))
            {
                baseWeight *= PositiveConnectionsTuning.CampfireModifier;
            }
        }

        // 5. Adjust final weight based on faction (1/5 as likely for non-colony interactions)
        float finalWeight = initiator.Faction == recipient.Faction ? baseWeight : baseWeight * 0.2f;

        CurrentRandomSelectionWeight = finalWeight;
        return finalWeight;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        // Initialize output parameters to null
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;

        // Apply thoughts to the storyteller (initiator) and the audience (recipient)
        ApplyStorytellingThoughts(initiator, recipient);

        // Generate a storytelling message using the utility method
        string storytellingMessage = PositiveConnectionsUtility.GenerateStorytellingMessage(initiator, recipient);

        if (!_modSettings.DisableAllMessages)
        {
            Messages.Message(storytellingMessage, new LookTargets(new Pawn[] { initiator, recipient }), MessageTypeDefOf.PositiveEvent);
        }

        OnPositiveInteraction?.Invoke(initiator, 0.25f, "PositiveInteraction", (int)ExperienceValency.Positive);
        
        if (_modSettings.EnableLogging)
        {
            string logMessage = $"<color=#00FF7F>[Positive Connections]</color> StorytellingInteraction - Weight: {CurrentRandomSelectionWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
            Log.Message(logMessage);
        }

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