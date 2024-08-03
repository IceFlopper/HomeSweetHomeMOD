using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HomeSweetHome
{
    [HarmonyPatch(typeof(Caravan), "AddPawn")]
    public static class Caravan_Depart_Patch
    {
        public static void Postfix(Caravan __instance, Pawn p)
        {
            if (__instance?.Faction == Faction.OfPlayer)
            {
                var tracker = Find.World.GetComponent<CaravanDepartureTracker>();
                if (tracker == null)
                {
                    Log.Error("HomeSweetHome: Failed to get CaravanDepartureTracker component.");
                    return;
                }
                tracker.SetDepartureTicks(__instance, Find.TickManager.TicksGame);
            }
        }
    }
}
