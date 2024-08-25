using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    public class StorytellingGameComponent : GameComponent
    {
        private int tickCounter = 0;
        private const int TickInterval = 2500; // Interval in ticks (approx. 1 game hour)

        private int gatheringDurationTicks = -1; // Default to -1 indicating no gathering

        public int GatheringDurationTicks => gatheringDurationTicks; // Public getter to access the field

        public StorytellingGameComponent(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            tickCounter++;
            if (tickCounter >= TickInterval)
            {
                tickCounter = 0; // Reset the counter
                // I comment out the following because we're using the native TryStartRandomGathering (or whatever) method
                // StorytellingUtility.CheckAndTriggerStorytelling();
            }
        }

        public void StartNewStorytelling(int duration)
        {
            gatheringDurationTicks = duration;
        }

        public void EndCurrentStorytelling()
        {
            gatheringDurationTicks = -1;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
            Scribe_Values.Look(ref gatheringDurationTicks, "gatheringDurationTicks", -1); // This will now work as it's a field
        }
    }
}