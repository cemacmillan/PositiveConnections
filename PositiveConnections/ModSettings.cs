using Verse;

namespace dIl_PositiveConnections
{
    public class ModSettings : Verse.ModSettings
    {
        public bool EnableGenderAdjustment;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableGenderAdjustment, "EnableGenderAdjustment", true);
        }
    }
}
