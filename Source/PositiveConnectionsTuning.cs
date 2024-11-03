// Behold! No Using statements!

namespace PositiveConnectionsNmSpc
{
    public static class PositiveConnectionsTuning
    {
        public static float BaseSelectionWeight_Compliment = 0.04f;
        public static float BaseSelectionWeight_DiscussIdeoligion = 0.02f;
        public static float BaseSelectionWeight_Gift = 0.05f;
        public static float BaseSelectionWeight_GiveComfort = 0.0667f;
        public static float BaseSelectionWeight_Mediation = 0.015f;
        public static float BaseSelectionWeight_SharedPassion = 0.02f;
        public static float BaseSelectionWeight_SkillShare = 0.02f;
        public static float BaseSelectionWeight_Storytelling = 0.02f;

        public static float DifferentIdeoFactor_DiscussIdeoligion = 0.2f;  // Factor when ideologies differ
        public static float NoIdeologyFactor_DiscussIdeoligion = 0.025f;   // Factor when Ideology DLC is inactive
        public static float NonColonyPawnFactor_SkillShare = 0.2f;
        public static float NonColonyPawnFactor_GiveComfort = 0.05f;
        public static float NonColonyPawnFactor = 0.33f; // increased 50%
        
        public static int SkillDifferenceDivisor_SkillShare = 5;
        public static int MinMoodForComfort = 45; // Was 45
        public static int MinSocialSkillRequired_GiveComfort = 1; // Reduced by 2/3
        public static int MinSocialSkillRequired_Mediation = 3; // Minimum social skill for Mediation
        public static int NegativeRelationshipThreshold_Mediation = -5; // Relationship threshold for Mediation
        public static int PassionLevelFactor_SharedPassion = 1; // halved
        
        public static int MaxMediationBonus = 12;
        public static int MinMediationBonus = 3;
        public static float NonColonyPawnFactor_SharedPassion = 0.2f;

        // Storytelling additional variables
        public static int MinSocialSkillRequired_Storytelling = 3;
        public static int MaxListenings_Storytelling = 5;
        public static int StorytellingRange = 12;
        public static float DenRoomRoleModifier = 1.5f;
        public static float CampfireModifier = 2.0f;

        // New variables for various conditions
        public static int MinIntellectualSkillRequired_IdeologicalDiscussion = 3; // Minimum Intellectual skill for initiator
        public static int MaxMemoryStack_Compliment = 3; // Maximum stackable memories for Compliment
        public static int MaxMemoryStack_GiveComfort = 5; // Maximum stackable memories for Give Comfort
        public static int MinSkillLevel_SkillShare = 5; // Minimum skill level for initiator to share skills
        public static int MinSkillLevel_SkillShareRecipient = 1; // Minimum skill level for initiator to share skills
        // public static bool KindPawnsAlwaysMediate = true; // Allow Kind pawns to mediate regardless of skill level
    }
}