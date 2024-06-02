using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnectionsModSettings : Verse.ModSettings
    {
        public bool EnableGenderAdjustment = true;
        public bool DisableAllMessages = false;
        public float BaseInteractionFrequency = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableGenderAdjustment, "EnableGenderAdjustment", true);
            Scribe_Values.Look(ref DisableAllMessages, "DisableAllMessages", false);
            Scribe_Values.Look(ref BaseInteractionFrequency, "BaseInteractionFrequency", 1f);
        }
    }
}