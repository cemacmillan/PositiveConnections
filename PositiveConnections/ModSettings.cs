using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnectionsModSettings : Verse.ModSettings
    {
        public bool EnableGenderAdjustment = true;
        public bool DisableAllMessages = false;
        public float BaseInteractionFrequency = 1f;
        public bool EnableLogging = false; // New setting for logging control

        // New settings
        public float MaxInteractionFrequency = 2.0f; // Max interaction frequency for small colonies
        public float MinInteractionFrequency = 0.25f; // Min interaction frequency for large colonies
        public bool StopInteractions = false; // Checkbox to stop interactions
        public bool EnableRareInteractions = true; // Checkbox to enable rare interactions
        public bool DisableCertainInteractions = false; // Checkbox to disable certain interactions

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableGenderAdjustment, "EnableGenderAdjustment", true);
            Scribe_Values.Look(ref DisableAllMessages, "DisableAllMessages", false);
            Scribe_Values.Look(ref BaseInteractionFrequency, "BaseInteractionFrequency", 1f);
            Scribe_Values.Look(ref MaxInteractionFrequency, "MaxInteractionFrequency", 2.0f);
            Scribe_Values.Look(ref MinInteractionFrequency, "MinInteractionFrequency", 0.25f);
            Scribe_Values.Look(ref StopInteractions, "StopInteractions", false);
            Scribe_Values.Look(ref EnableRareInteractions, "EnableRareInteractions", true);
            Scribe_Values.Look(ref DisableCertainInteractions, "DisableCertainInteractions", false);
            Scribe_Values.Look(ref EnableLogging, "EnableLogging", false); // Save/Load EnableLogging
        }
    }
}