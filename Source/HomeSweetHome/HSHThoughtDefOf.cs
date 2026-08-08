using RimWorld;
using Verse;

namespace HomeSweetHome
{
    [DefOf]
    public static class HSHThoughtDefOf
    {
        // Somebody you're separated from, one per absent pawn.
        public static ThoughtDef HSH_AwayPartner;
        public static ThoughtDef HSH_AwayChild;
        public static ThoughtDef HSH_AwayParent;
        public static ThoughtDef HSH_AwaySibling;
        public static ThoughtDef HSH_AwayRelative;
        public static ThoughtDef HSH_AwayCloseFriend;
        public static ThoughtDef HSH_AwayFriend;
        public static ThoughtDef HSH_AwayRival;
        public static ThoughtDef HSH_AwayEnemy;

        // ...and the moment they walk back through the gate.
        public static ThoughtDef HSH_ReturnedPartner;
        public static ThoughtDef HSH_ReturnedFamily;
        public static ThoughtDef HSH_ReturnedFriend;
        public static ThoughtDef HSH_ReturnedRival;
        public static ThoughtDef HSH_ReturnedEnemy;

        // The traveller's own state of mind.
        public static ThoughtDef HSH_Homesick;
        public static ThoughtDef HSH_HomeAtLast;
        public static ThoughtDef HSH_ReachedDestination;
        public static ThoughtDef HSH_MadeCamp;

        // Who they're stuck on the road with.
        public static ThoughtDef HSH_RoadPartner;
        public static ThoughtDef HSH_RoadFamily;
        public static ThoughtDef HSH_RoadFriend;
        public static ThoughtDef HSH_RoadRival;
        public static ThoughtDef HSH_RoadEnemy;

        // Opinion shifts left behind by a shared trip.
        public static ThoughtDef HSH_SharedTheRoad;
        public static ThoughtDef HSH_EnduredEachOther;

        static HSHThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HSHThoughtDefOf));
        }
    }
}
