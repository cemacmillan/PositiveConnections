using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    public static class PositiveConnectionsTuning
    {
       
        public static float BaseSelectionWeight_Compliment = 0.085f;
        public static float BaseSelectionWeight_DiscussIdeoligion = 0.0075f;
        public static float BaseSelectionWeight_Gift = 0.05f;
        public static float BaseSelectionWeight_GiveComfort = 0.0667f;
        public static float BaseSelectionWeight_Mediation = 0.015f;
        public static float BaseSelectionWeight_SharedPassion = 0.002f;
        public static float BaseSelectionWeight_SkillShare = 0.005f;


        public static float NonColonyPawnFactor_SkillShare = 0.2f;
        public static int SkillDifferenceDivisor_SkillShare = 10;
        public static int MinMoodForComfort = 40; // Was 45
        public static int MinSocialSkillRequired_GiveComfort = 1; // Reduced by 2/3
        public static int MinSocialSkillRequired_Mediation = 3; // Minimum social skill for Mediation
        public static int NegativeRelationshipThreshold_Mediation = -8; // Relationship threshold for Mediation
        public static int PassionLevelFactor_SharedPassion = 1; // halved
        public static float NonColonyPawnFactor = 0.33f; // increased 50%
        public static int MaxMediationBonus = 12;
        public static int MinMediationBonus = 3;
        public static float NonColonyPawnFactor_SharedPassion = 0.2f;


        // Storytelling
        public static float BaseSelectionWeight_Storytelling = 0.1f;
        public static int MinSocialSkillRequired_Storytelling = 3;
        public static int StorytellingRange = 12;
        public static float DenRoomRoleModifier = 1.5f;
        public static float CampfireModifier = 2.0f;
    }


      


}