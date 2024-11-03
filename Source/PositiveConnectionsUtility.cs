using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace PositiveConnectionsNmSpc;

public enum ExperienceValency
{
    Positive,
    Negative,
    Neutral
}

public static class PositiveConnectionsUtility
{
    // Check if both pawns are relevant to the player's colony
    public static bool ArePawnsRelevant(Pawn initiator, Pawn recipient)
    {
        if (initiator == null || recipient == null || initiator == recipient)
            return false;

        if (!initiator.RaceProps.Humanlike || !recipient.RaceProps.Humanlike)
            return false;

        if (initiator.Dead || recipient.Dead)
            return false;

        // Ensure at least one pawn is from the player's faction
        return initiator.Faction == Faction.OfPlayer || recipient.Faction == Faction.OfPlayer;
    }

    // Calculate base weight for interaction selection
    public static float CalculateBaseWeight(Pawn initiator, float baseSelectionWeight,
        PositiveConnectionsModSettings settings, bool applyMoodModifier = false)
    {
        int colonySize = initiator.Map.mapPawns.FreeColonistsCount;
        float adjustedFrequency = AdjustInteractionFrequency(colonySize, settings);

        // Calculate base weight with or without the mood modifier
        float weight = baseSelectionWeight * adjustedFrequency * settings.BaseInteractionFrequency;

        if (applyMoodModifier)
        {
            weight *= initiator.needs.mood.CurLevel;
        }

        return weight;
    }

    // Adjust interaction frequency based on colony size
    public static float AdjustInteractionFrequency(int colonySize, PositiveConnectionsModSettings settings)
    {
        if (settings.StopInteractions)
        {
            return 0.00001f; // Effectively stops interactions
        }

        float maxFrequency = settings.MaxInteractionFrequency;
        float minFrequency = settings.MinInteractionFrequency;

        // Inverse scaling function
        float scalingFactor = maxFrequency * 2 / Mathf.Sqrt(colonySize);

        // Clamp the value between min and max frequencies
        return Mathf.Clamp(scalingFactor, minFrequency, maxFrequency);
    }
    
    // New methods which test for conditions in RandomSelectionWeight
    
    // Test for hold down on interactions which allow only one memory
    public static bool HasMemory(Pawn pawn, ThoughtDef def)
    {
        // Log.Message($"testing HasMemory: pawn={pawn}, def={def}");
        return pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(def) != null;
    }
    
    // Test for hold down on memories which allow stacking
    public static bool HasLimitedMemories(Pawn pawn, ThoughtDef def, int maxAllowed)
    {
        int memoryCount = pawn.needs.mood.thoughts.memories.NumMemoriesOfDef(def);
        return memoryCount >= maxAllowed; // Return true if the limit has been reached or exceeded
    }
    
    // Check if initiator has sufficient Intellectual to do at least a mediocre job of explaining it to another
    public static bool IsEligibleForIdeologicalDiscussion(Pawn initiator, Pawn recipient)
    {
        // Ensure that the initiator has a sufficient Intellectual skill level
        if (initiator.skills.GetSkill(SkillDefOf.Intellectual).Level < PositiveConnectionsTuning.MinIntellectualSkillRequired_IdeologicalDiscussion)
        {
            return false;
        }

        // Check that both initiator and recipient are capable of Social work
        return !initiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled &&
               !recipient.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
    }
    
    // Test if a pawn might be a capable mediator. This also helps prevent poor mediation outcomes
    public static bool IsSuitedForMediation(Pawn initiator)
    {
        // Define the traits that would make a pawn unsuited for mediation
        List<string> unsuitedTraits = new List<string> { "Psychopath", "MM_Desensitized", "VTE_WorldWeary" };

        // Check if the initiator has any disqualifying traits
        if (PositiveConnectionsTraitMgr.PawnHasAnyTrait(initiator, unsuitedTraits))
        {
            return false;
        }

        // Check if the pawn is "Kind" or has the required social skill level and Caring capability
        return PositiveConnectionsTraitMgr.PawnHasTrait(initiator, "Kind") ||
               (initiator.skills.GetSkill(SkillDefOf.Social).Level >= PositiveConnectionsTuning.MinSocialSkillRequired_Mediation &&
                !initiator.WorkTagIsDisabled(WorkTags.Caring));
    }

    
    // Test if the initiator is too barjo to give compliments and is not kind
    public static bool CanCareAboutOther(Pawn initiator)
    {
        // Define lists of positive and negative traits for caring ability
        List<string> positiveTraits = new List<string> { "Kind", "MM_Empathetic", "SP_Caring", "SP_Empathetic" };
        List<string> negativeTraits = new List<string> { "MM_SelfCentered", "MM_Desensitized", "Psychopath", "VTE_WorldWeary" };

        // Check for negative traits first; return false if any are present
        if (PositiveConnectionsTraitMgr.PawnHasAnyTrait(initiator, negativeTraits))
        {
            return false;
        }

        // Check for any positive traits; return true if any are found
        if (PositiveConnectionsTraitMgr.PawnHasAnyTrait(initiator, positiveTraits))
        {
            return true;
        }

        // If no positive or negative traits are present, return true by default
        return true;
    }
    
    // Test if pawn can profit from SkillSharing
    public static bool CanReceiveSkillShare(Pawn recipient, SkillDef skill, int minSkillLevel)
    {
        return !recipient.skills.GetSkill(skill).TotallyDisabled && recipient.skills.GetSkill(skill).Level >= minSkillLevel;
    }
    
    // Change faction relations
    public static void ChangeFactionRelations(Faction factionA, Faction factionB, int goodwillChange)
    {
        if (goodwillChange == 0)
        {
            goodwillChange = Rand.RangeInclusive(1, 15);
        }
        else
        {
            goodwillChange = Mathf.Clamp(goodwillChange, -100, 100);
        }

        FactionRelation factionRelationA = factionA.RelationWith(factionB);
        FactionRelation factionRelationB = factionB.RelationWith(factionA);

        factionRelationA.baseGoodwill = Mathf.Clamp(factionRelationA.baseGoodwill + goodwillChange, -100, 100);
        factionRelationB.baseGoodwill = factionRelationA.baseGoodwill;

        factionRelationA.CheckKindThresholds(factionA, canSendLetter: false, null, GlobalTargetInfo.Invalid, out _);
        factionRelationB.CheckKindThresholds(factionB, canSendLetter: false, null, GlobalTargetInfo.Invalid, out _);
    }

    // Generate gift description using RulePack system
    public static string GenerateGiftDescription(Pawn initiator, Pawn recipient)
    {
        // Create a GrammarRequest
        GrammarRequest request = new GrammarRequest();

        // Include your custom RulePacks
        request.Includes.Add(RulePackDef.Named("PositiveConnections_GiftMessage"));
        request.Includes.Add(RulePackDef.Named("PositiveConnections_GiftDescription"));

        // Add rules for initiator and recipient
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));

        // Generate the message using the root symbol 'giftMessage'
        string message = GrammarResolver.Resolve("giftMessage", request);

        return message;
    }

    // Generate compliment message
    public static string GenerateComplimentMessage(Pawn initiator, Pawn recipient)
    {
        GrammarRequest request = new GrammarRequest();

        // Include your custom RulePacks for compliments
        request.Includes.Add(RulePackDef.Named("PositiveConnections_ComplimentMessage"));
        request.Includes.Add(RulePackDef.Named("PositiveConnections_ComplimentSubject"));

        // Add rules for initiator and recipient
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));

        // Generate the message using the root symbol 'complimentMessage'
        string message = GrammarResolver.Resolve("complimentMessage", request);

        return message;
    }

    // Generate shared passion message
    public static string GenerateSharedPassionMessage(Pawn pawnA, Pawn pawnB, SkillRecord sharedPassion)
    {
        // Create a GrammarRequest
        GrammarRequest request = new GrammarRequest();

        // Include custom RulePacks
        request.Includes.Add(RulePackDef.Named("PositiveConnections_SharedPassionMessage"));
        request.Includes.Add(RulePackDef.Named("PositiveConnections_SharedPassionAction"));

        // Add rules for pawnA and pawnB
        request.Rules.AddRange(GrammarUtility.RulesForPawn("pawnA", pawnA));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("pawnB", pawnB));

        // Add constants for passionSkill and passionSubject
        string skillDefName = sharedPassion.def.defName; // E.g., "Construction"
        string skillLabel = sharedPassion.def.skillLabel.CapitalizeFirst(); // E.g., "Construction"

        request.Rules.Add(new Rule_String("passionSkill", skillDefName));
        request.Rules.Add(new Rule_String("passionSubject", skillLabel));

        // Generate the message using the root symbol 'sharedPassionMessage'
        string message = GrammarResolver.Resolve("sharedPassionMessage", request);

        return message;
    }
    
    // new method for skill share interaction messages
    public static string GenerateSkillShareMessage(Pawn initiator, Pawn recipient, SkillDef skillDef)
    {
        GrammarRequest request = new GrammarRequest();

        // Include the SkillShare message and articles RulePacks
        request.Includes.Add(RulePackDef.Named("PositiveConnections_SkillShareMessage"));
        request.Includes.Add(RulePackDef.Named("PositiveConnections_SkillArticles"));

        // Add rules for initiator and recipient
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));

        // Add the skill label and defName
        string skillLabel = skillDef.skillLabel.ToLower(); // Use lowercase for articles
        string skillDefName = skillDef.defName;

        request.Rules.Add(new Rule_String("skillLabel", skillLabel));
        request.Rules.Add(new Rule_String("skillDefName", skillDefName));

        // Generate the message using the root symbol 'skillShareMessage'
        string message = GrammarResolver.Resolve("skillShareMessage", request);

        return message;
    }
    
    // new method for ideological discussion interaction message
    public static string GenerateIdeologicalDiscussionMessage(Pawn initiator, Pawn recipient)
    {
        GrammarRequest request = new GrammarRequest();

        // Check if Ideology is active
        if (ModsConfig.IdeologyActive)
        {
            request.Includes.Add(RulePackDef.Named("PositiveConnections_IdeologyDiscussionMessage_IdeoActive"));

            // Add Ideo rules if necessary
            request.Rules.Add(new Rule_String("ideoName", initiator.Ideo.name));
        }
        else
        {
            request.Includes.Add(RulePackDef.Named("PositiveConnections_IdeologyDiscussionMessage_IdeoInactive"));
        }

        // Add rules for initiator and recipient
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));

        // Generate the message using the root symbol 'ideologyDiscussionMessage'
        string message = GrammarResolver.Resolve("ideologyDiscussionMessage", request);

        return message;
    }
    
    public static string GenerateComfortMessage(Pawn initiator, Pawn recipient)
    {
        GrammarRequest request = new GrammarRequest();

        // Include the comfort message RulePack
        request.Includes.Add(RulePackDef.Named("PositiveConnections_ComfortMessage"));

        // Add rules for initiator and recipient
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));

        // Generate the message using the root symbol 'comfortMessage'
        string message = GrammarResolver.Resolve("comfortMessage", request);

        return message;
    }
    
    public static string GenerateMediationMessage(Pawn initiator, Pawn recipient, Pawn conflictPawn)
    {
        // Since we only need one message variation, we can generate it directly
        // Alternatively, for consistency, we can use the RulePack system

        GrammarRequest request = new GrammarRequest();

        // Include the mediation message RulePack
        request.Includes.Add(RulePackDef.Named("PositiveConnections_MediationMessage"));

        // Add rules for initiator, recipient, and conflictPawn
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("conflictPawn", conflictPawn));

        // Generate the message using the root symbol 'mediationMessage'
        string message = GrammarResolver.Resolve("mediationMessage", request);

        return message;
    }
    
    public static string GenerateStorytellingMessage(Pawn initiator, Pawn recipient)
    {
        GrammarRequest request = new GrammarRequest();

        // Include the storytelling message RulePack
        request.Includes.Add(RulePackDef.Named("PositiveConnections_StorytellingMessage"));

        // Add rules for initiator and recipient
        request.Rules.AddRange(GrammarUtility.RulesForPawn("initiator", initiator));
        request.Rules.AddRange(GrammarUtility.RulesForPawn("recipient", recipient));

        // Add a random subject
        string storySubject = GetRandomStorySubject();
        request.Rules.Add(new Rule_String("storySubject", storySubject));

        // Generate the message using the root symbol 'storytellingMessage'
        string message = GrammarResolver.Resolve("storytellingMessage", request);

        return message;
    }

    private static string GetRandomStorySubject()
    {
        List<string> subjectKeys = new List<string>
        {
            "DIL_StorySubject_1",
            "DIL_StorySubject_2",
            "DIL_StorySubject_3",
            "DIL_StorySubject_4",
            "DIL_StorySubject_5",
            "DIL_StorySubject_6",
            "DIL_StorySubject_7",
            "DIL_StorySubject_8",
            "DIL_StorySubject_9",
            "DIL_StorySubject_10",
            "DIL_StorySubject_11",
            "DIL_StorySubject_12",
            "DIL_StorySubject_13",
            "DIL_StorySubject_14",
            "DIL_StorySubject_15",
            "DIL_StorySubject_16",
            "DIL_StorySubject_17",
            "DIL_StorySubject_18",
            "DIL_StorySubject_19",
            "DIL_StorySubject_20"
        };

        string randomKey = subjectKeys.RandomElement();
        return randomKey.Translate();
    }

}