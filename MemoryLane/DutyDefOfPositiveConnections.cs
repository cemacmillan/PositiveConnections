using RimWorld;
using Verse;
using Verse.AI;

namespace DIL_PositiveConnections
{
	[DefOf]
	public static class PositiveConnectionsDutyDefOf
	{
	
		public static DutyDef DIL_Duty_Storytelling; //now unused
        public static DutyDef DIL_Duty_Storytelling_Organizer;
        public static DutyDef DIL_Duty_Storytelling_Attendee;
       

        /*
                I can never remember syntax for MayRequireDLC. Now I know where to find it.
                [MayRequireRoyalty]
                public static DutyDef Bestow;
        */

        static PositiveConnectionsDutyDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DutyDefOf));
		}
	}

    [DefOf]
    public static class PositiveConnectionsJobDefOf
    {

        public static JobDef TellStory;

        public static JobDef ListenToStory;

        static PositiveConnectionsJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DutyDefOf));
        }
    }
}
