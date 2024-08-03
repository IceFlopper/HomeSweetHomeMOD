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
            if (__instance.Faction == Faction.OfPlayer)
            {
                Log.Message($"HomeSweetHome: Pawn {p.Name.ToStringShort} added to caravan.");

                // Store the departure ticks when the first pawn is added to the caravan
                CaravanDepartureTracker tracker = Find.World.GetComponent<CaravanDepartureTracker>();
                if (tracker != null)
                {
                    tracker.SetDepartureTicks(__instance, Find.TickManager.TicksGame);
                }
            }
        }
    }
}
